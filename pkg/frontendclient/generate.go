package frontendclient

//go:generate protoc -I ../../protos/ ../../protos/frontend.proto --go_out=import_path=github.com/simonferquel/AppSDK/pkg/frontendclient,plugins=grpc:.
