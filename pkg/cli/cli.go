package cli

import (
	"github.com/simonferquel/AppSDK/pkg/frontendclient"
	"google.golang.org/grpc"
)

type Cli struct {
	FrontendCli frontendclient.AppFrontendClient
}

func (c *Cli) Initialize(address string) error {
	conn, err := grpc.Dial(address, grpc.WithInsecure())
	if err != nil {
		return err
	}
	c.FrontendCli = frontendclient.NewAppFrontendClient(conn)
	return nil
}
