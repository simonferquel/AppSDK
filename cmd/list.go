package main

import (
	"context"
	"fmt"
	"os"
	"strings"
	"text/tabwriter"

	"github.com/simonferquel/AppSDK/pkg/cli"
	"github.com/simonferquel/AppSDK/pkg/frontendclient"
	"github.com/spf13/cobra"
)

type listOptions struct {
	quiet bool
}

func newListCommand(cli *cli.Cli) *cobra.Command {
	opts := &listOptions{}
	cmd := &cobra.Command{
		Use:     "list",
		Aliases: []string{"ls"},
		Short:   "list applications exposed by the frontend",
		RunE: func(cmd *cobra.Command, args []string) error {
			resp, err := cli.FrontendCli.ListApps(context.Background(), &frontendclient.StringMessage{})
			if err != nil {
				return err
			}
			if opts.quiet {
				for _, app := range resp.Apps {
					fmt.Println(strings.Join(app.Names, ", "))
				}
			} else {
				w := tabwriter.NewWriter(os.Stdout, 0, 1, 3, byte(' '), 0)
				defer w.Flush()
				w.Write([]byte("NAME\tVERSION\tAUTHOR\tDESCRIPTION\n"))
				for _, app := range resp.Apps {
					fmt.Fprintf(w, "%s\t%s\t%s\t%s\n", strings.Join(app.Names, ", "), app.Version, app.Author, app.Description)
				}
			}

			return nil
		},
	}
	cmd.Flags().BoolVarP(&opts.quiet, "quiet", "q", false, "only print a list of names")
	return cmd
}
