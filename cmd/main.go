package main

import (
	"github.com/simonferquel/AppSDK/pkg/cli"
	"github.com/spf13/cobra"
)

type rootOptions struct {
	address string
}

func main() {
	opts := &rootOptions{}
	cli := &cli.Cli{}
	root := &cobra.Command{
		Short: "Command line for managing apps",
		PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
			return cli.Initialize(opts.address)
		},
	}
	root.AddCommand(
		newListCommand(cli),
		newGenSettingsCommand(cli),
		newDeployCommand(cli),
	)
	root.PersistentFlags().StringVar(&opts.address, "frontend", "", "frontend address")
	if err := root.Execute(); err != nil {
		panic(err)
	}
}
