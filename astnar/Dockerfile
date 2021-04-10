FROM ubuntu:20.04

RUN apt update

# Install the dotnet SDK
RUN apt install -y wget

RUN wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN  dpkg -i packages-microsoft-prod.deb

RUN  apt-get update;  apt-get install -y apt-transport-https &&  apt-get update &&  apt-get install -y dotnet-sdk-5.0


# Install GCC and Make

RUN apt install -y build-essential


# Copy source code into image
COPY ./vinci ./vinci

COPY ./tool ./tool

# Build Vinci 

WORKDIR  vinci

RUN make

# Build main Tool

WORKDIR ./../tool

# RUN dotnet restore
RUN dotnet restore
RUN dotnet build -c release -o app --no-restore


# Copy the vinci executable to the dotnet app folder

RUN cp ./../vinci/vinci .


ENTRYPOINT ["./app/ASTNAR"]