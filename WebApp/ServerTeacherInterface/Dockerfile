FROM node:16

# Create app directory
WORKDIR /usr/src/app/webTeacherArete

# Install app dependencies
COPY package.json yarn.lock /usr/src/app/webTeacherArete/
RUN yarn install

# Bundle app source
COPY . /usr/src/app/webTeacherArete
# RUN yarn build

EXPOSE 4200
CMD [ "yarn", "start" ]