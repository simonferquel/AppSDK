FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY csharp/AppSDK.sln ./
COPY csharp/Docker.WebStore/Docker.WebStore.csproj Docker.WebStore/
COPY csharp/Docker.AppSDK/Docker.AppSDK.csproj Docker.AppSDK/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY csharp .
WORKDIR /src/Docker.WebStore
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM golang:1.10-stretch AS golang-builder
WORKDIR /go/src/github.com/simonferquel/AppSDK
COPY cmd cmd
COPY pkg pkg
COPY vendor vendor
RUN go build -o /backend ./cmd

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY --from=golang-builder /backend /backend
ENV BackendService.Exe /backend
ENTRYPOINT ["dotnet", "Docker.WebStore.dll"]
