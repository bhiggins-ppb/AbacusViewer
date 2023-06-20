# This file is a work in progress and can be safely ignored during the .net core upgrade PR review. 
# Building this container is a pre-requisite to spinning up the docker-compose system. 
# This file will be in an future MR which will enable our new pipeline. 

# TODO: this is the wrong image: we should be using the RUNTIME not the SDK
#       will fix later.
FROM mcr.microsoft.com/dotnet/sdk:6.0

RUN apt update && apt install -y telnet

WORKDIR /src
EXPOSE 5023

COPY ./bin/Debug/net6.0/publish .

ENTRYPOINT ["dotnet", "KestrelTest.dll"]