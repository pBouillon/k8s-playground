# Pull the official base image
FROM node:16-alpine3.11 as build

# Set your working directory
WORKDIR /usr/src/app

# Add `/usr/src/app/node_modules/.bin` to $PATH
ENV PATH /usr/src/app/node_modules/.bin:$PATH

# Install application dependencies
COPY package*.json ./
RUN npm install --silent

# Add app
COPY . .

# Will build app
RUN npm run build --prod



# Will serve it from nginx
FROM nginx:1.21-alpine

COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /usr/src/app/dist/consumer /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
