package main

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io/ioutil"
	"os"

	v1beta2cli "github.com/docker/cli/kubernetes/client/clientset/typed/compose/v1beta2"
	"github.com/docker/cli/kubernetes/compose/v1beta2"
	"github.com/simonferquel/AppSDK/pkg/cli"
	"github.com/simonferquel/AppSDK/pkg/frontendclient"
	"github.com/spf13/cobra"
	yaml "gopkg.in/yaml.v2"
	"k8s.io/client-go/tools/clientcmd"
)

type deployOptions struct {
	settingsFile         string
	name                 string
	runtimeConfigFile    string
	renderOnly           bool
	genRuntimeParameters bool
}

func (o *deployOptions) resolve() (*resolvedOptions, error) {
	if o.name == "" {
		return nil, errors.New("name is required")
	}
	res := &resolvedOptions{
		genRuntimeParameters: o.genRuntimeParameters,
		name:                 o.name,
		renderOnly:           o.renderOnly,
	}
	if o.settingsFile == "-" {
		data, err := ioutil.ReadAll(os.Stdin)
		if err != nil {
			return nil, err
		}
		res.settings = string(data)
	} else if o.settingsFile != "" {
		data, err := ioutil.ReadFile(o.settingsFile)
		if err != nil {
			return nil, err
		}
		res.settings = string(data)
	}
	var rawRuntimeConfig []byte
	if o.runtimeConfigFile == "-" {
		data, err := ioutil.ReadAll(os.Stdin)
		if err != nil {
			return nil, err
		}
		rawRuntimeConfig = data
	} else if o.runtimeConfigFile != "" {
		data, err := ioutil.ReadFile(o.runtimeConfigFile)
		if err != nil {
			return nil, err
		}
		rawRuntimeConfig = data
	}
	if rawRuntimeConfig != nil {
		d := yaml.NewDecoder(bytes.NewReader(rawRuntimeConfig))
		res.runtimeConfig = make(map[interface{}]interface{})
		if err := d.Decode(&res.runtimeConfig); err != nil {
			return nil, err
		}
	}
	return res, nil
}

type resolvedOptions struct {
	settings             string
	name                 string
	runtimeConfig        map[interface{}]interface{}
	renderOnly           bool
	genRuntimeParameters bool
}

func queryRuntimeConfig(rc map[interface{}]interface{}, path ...string) (interface{}, bool) {
	if rc == nil {
		return nil, false
	}
	if len(path) == 0 {
		return rc, true
	}
	token, ok := rc[path[0]]
	if !ok {
		return nil, false
	}
	if len(path) == 1 {
		return token, true
	}
	subRC, ok := token.(map[interface{}]interface{})
	if !ok {
		return nil, false
	}
	return queryRuntimeConfig(subRC, path[1:]...)
}

func convEnv(src map[string]string) map[string]*string {
	res := map[string]*string{}
	for k, v := range src {
		res[k] = &v
	}
	return res
}

func convScalabilityModel(model frontendclient.ScalabilityModel) v1beta2.DeployConfig {
	var replicas uint64 = 1
	switch model {
	case frontendclient.ScalabilityModel_Global:
		return v1beta2.DeployConfig{Mode: "global"}
	default:
		return v1beta2.DeployConfig{Mode: "replicated", Replicas: &replicas}
	}
}

func convPorts(src []*frontendclient.ExposedPort) []v1beta2.ServicePortConfig {
	res := []v1beta2.ServicePortConfig{}
	for _, p := range src {
		c := v1beta2.ServicePortConfig{
			Published: uint32(p.PublicPort),
			Target:    uint32(p.ContainerPort),
		}
		switch p.Kind {
		case frontendclient.PortKind_TCP:
			c.Protocol = "TCP"
		case frontendclient.PortKind_UDP:
			c.Protocol = "UDP"
		}
		res = append(res, c)
	}
	return res
}

func newDeployCommand(cli *cli.Cli) *cobra.Command {
	opts := &deployOptions{}
	cmd := &cobra.Command{
		Use:  "deploy [options] <app name>",
		Args: cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			o, err := opts.resolve()
			if err != nil {
				return err
			}
			resp, err := cli.FrontendCli.RenderApp(context.Background(),
				&frontendclient.RenderAppRequest{
					Name:            args[0],
					ParameterValues: o.settings,
				})
			if err != nil {
				return err
			}
			if opts.renderOnly {
				js, err := json.MarshalIndent(resp, "", "  ")
				if err != nil {
					return err
				}
				fmt.Println(string(js))

				if o.runtimeConfig != nil {
					fmt.Printf("%#v\n", o.runtimeConfig)
				}
				return nil
			}
			if opts.genRuntimeParameters {
				fmt.Print("runtime:\n")
				for _, svc := range resp.Services {
					fmt.Printf("  %s:\n", svc.Name)
					if svc.ScalabilityModel == frontendclient.ScalabilityModel_Scalable {
						fmt.Print("    scale: 1\n")
					}
					fmt.Printf("    limits:\n")
					fmt.Printf("      cpus: # cpu limit in nanocpu\n")
					fmt.Printf("      memory: # memory limit in bytes\n")
					fmt.Printf("    reservations:\n")
					fmt.Printf("      cpus: # cpu limit in nanocpu\n")
					fmt.Printf("      memory: # memory limit in bytes\n")
				}
				return nil
			}

			stack := &v1beta2.Stack{}
			stack.Name = o.name
			stack.Spec = &v1beta2.StackSpec{}
			for _, svc := range resp.Services {
				stackSvc := v1beta2.ServiceConfig{
					Name:        svc.Name,
					Image:       svc.Image,
					Environment: convEnv(svc.Env),
					Command:     svc.Command,
					Deploy:      convScalabilityModel(svc.ScalabilityModel),
					Ports:       convPorts(svc.Ports),
				}
				if svc.ScalabilityModel == frontendclient.ScalabilityModel_Scalable {
					if v, ok := queryRuntimeConfig(o.runtimeConfig, "runtime", svc.Name, "scale"); ok {
						val, ok := v.(int)
						if !ok {
							return errors.New("runtime scale invalid")
						}
						vuint64 := uint64(val)
						stackSvc.Deploy.Replicas = &vuint64
					}
				}
				// todo: limits, reservations, placements

				stack.Spec.Services = append(stack.Spec.Services, stackSvc)
			}

			cfg, err := clientcmd.BuildConfigFromFlags("", "c:\\users\\simon\\.kube\\config")
			if err != nil {
				return err
			}
			client, err := v1beta2cli.NewForConfig(cfg)
			if err != nil {
				return err
			}

			_, err = client.Stacks("default").Create(stack)
			return err
		},
	}
	cmd.Flags().StringVarP(&opts.settingsFile, "settings-file", "f", "", "settings file (- for stdin)")
	cmd.Flags().StringVarP(&opts.runtimeConfigFile, "runtime-config-file", "c", "", "runtime config file (- for stdin)")
	cmd.Flags().StringVarP(&opts.name, "name", "n", "", "name of the deployment")
	cmd.Flags().BoolVarP(&opts.renderOnly, "render-only", "r", false, "only render the service to stdout")
	cmd.Flags().BoolVarP(&opts.genRuntimeParameters, "gen-runtime-params", "g", false, "generate runtime parameters file")
	return cmd
}
