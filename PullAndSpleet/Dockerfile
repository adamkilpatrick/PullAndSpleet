FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM public.ecr.aws/lambda/dotnet:6
RUN yum -y groupinstall "Development Tools"
RUN yum install -y wget tar xz openssl-devel bzip2-devel libffi-devel libsndfile python-pip xz-devel

# Install ffmpeg
RUN mkdir -v -p /usr/local/bin/ffmpeg
WORKDIR /usr/local/bin/ffmpeg
RUN wget https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz
RUN tar -v -xf ffmpeg-release-amd64-static.tar.xz --strip-components=1 && \
rm -v -f ffmpeg-release-amd64-static.tar.xz && \
ln -snf /usr/local/bin/ffmpeg/ffmpeg /usr/bin/ffmpeg && \
ln -snf /usr/local/bin/ffmpeg/ffprobe /usr/bin/ffprobe

# Install python39 + youtube-dl + spleeter
RUN mkdir -v -p /tmp/python39
WORKDIR /tmp/python39
RUN wget https://www.python.org/ftp/python/3.9.10/Python-3.9.10.tgz && \
tar xvf Python-3.9.10.tgz && \
cd Python-*/ && \
./configure --enable-optimizations && \
make altinstall
RUN pip3.9 install --upgrade youtube-dl
RUN pip3.9 install --upgrade spleeter
ENV MODEL_PATH=/tmp
ENV NUMBA_CACHE_DIR=/tmp
RUN wget https://github.com/deezer/spleeter/raw/master/audio_example.mp3
RUN spleeter separate -p spleeter:4stems -o output audio_example.mp3

WORKDIR /App
COPY --from=build-env /App/out/* /var/task/
CMD [ "PullAndSpleet::Program::pullAndSpleet" ]