package main

import (
	"context"
	"fmt"

	"github.com/simonferquel/AppSDK/pkg/cli"
	"github.com/simonferquel/AppSDK/pkg/frontendclient"
	"github.com/spf13/cobra"
)

func newGenSettingsCommand(cli *cli.Cli) *cobra.Command {
	return &cobra.Command{
		Use:   "gen-settings <app name>",
		Short: "generate settings file for package",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			resp, err := cli.FrontendCli.GetAppSettingsTemplate(context.Background(),
				&frontendclient.StringMessage{Data: args[0]})
			if err != nil {
				return err
			}
			fmt.Println(resp.Data)
			return nil
		},
	}
}
