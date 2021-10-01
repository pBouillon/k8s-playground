import json
from random import uniform
import os
from time import sleep

from typing import Optional

import pika
import uvicorn

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware


app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

rabbitmq_host = os.getenv('RABBITMQ_HOST')


@app.get("/messages")
def read_root():
    with open('received.txt', 'r+') as f:
        content = f.readline()
    return json.loads(content) if content else []


def listen_to_rabbitmq():
    messages = []

    print('CHILD:   Waiting for RabbitMQ to be up and running')
    sleep(3)

    print('CHILD:   Connecting to RabbitMQ ...')
    while True:
        try:
            connection = pika.BlockingConnection(
                pika.ConnectionParameters(
                    rabbitmq_host if rabbitmq_host is not None else 'localhost'))
            break
        except pika.exceptions.AMQPConnectionError:
            sleep(1)
            print('CHILD:   Unable to connect, retrying ...')
            continue
    channel = connection.channel()
    print('CHILD:   Connected')

    channel.queue_declare(queue='Api.Message', durable=True)

    def callback(ch, method, properties, body):
        message = json.loads(body)
        delay = uniform(0, 2)

        print(f'CHILD:   Received {message}, sleeping for {delay} second')
        sleep(delay)

        messages.append(message)

        with open('received.txt', 'w') as f:
            json.dump(messages, f)

    while True:
        try:
            channel.basic_consume(queue='Api.Message', on_message_callback=callback, auto_ack=True)
            break
        except pika.exceptions.ChannelClosedByBroker:
            sleep(1)
            print('CHILD:   Error, retrying ...')
            continue

    print('CHILD:   Listening to the queue')

    channel.start_consuming()


if __name__ == "__main__":
    # Create the file
    with open('received.txt', 'a') as f:
        pass

    pid = os.fork()

    # Parent
    if pid != 0:
        uvicorn.run(app, host="0.0.0.0", port=8000)
    else:
        listen_to_rabbitmq()
