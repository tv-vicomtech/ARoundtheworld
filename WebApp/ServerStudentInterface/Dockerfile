FROM node:16

# Create app directory
WORKDIR /usr/src/app/webStudentArete

# Install app dependencies
COPY package.json yarn.lock /usr/src/app/webStudentArete/
RUN yarn install

# Bundle app source
COPY . /usr/src/app/webStudentArete
# RUN yarn build

EXPOSE 4210
CMD [ "yarn", "start" ]