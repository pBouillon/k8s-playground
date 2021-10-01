FROM python:3.9.7-alpine3.14

RUN python -m pip install --upgrade pip

WORKDIR /code

COPY . /code

RUN pip install -r requirements.txt

CMD ["python", "main.py"]
