FROM microsoft/dotnet:runtime
ENTRYPOINT ["dotnet", "Lykke.Service.OrderBooksCacheProvider.dll"]
ARG source=.
WORKDIR /app
COPY $source .