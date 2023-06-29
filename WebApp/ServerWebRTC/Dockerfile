FROM node:16

# Create app directory
WORKDIR /usr/src/app/ServerWebRTC

# Install app dependencies
COPY package.json yarn.lock /usr/src/app/ServerWebRTC/
RUN yarn install

# Bundle app source
COPY . /usr/src/app/ServerWebRTC
RUN yarn build

EXPOSE 80
CMD [ "yarn", "start" ]