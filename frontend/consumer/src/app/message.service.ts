import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'
import { Observable } from 'rxjs';
import { Message, MessageUpper } from './message';
import { map } from 'rxjs/operators';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(
    private readonly http: HttpClient,
  ) { }

  getMessages(): Observable<Message[]> {
    return this.http.get<Message[]>(environment.producerApi + '/messages');
  }

  getMessagesFromConsumerApi(): Observable<Message[]> {
    return this.http.get<MessageUpper[]>(environment.consumerApi + '/messages').pipe(
      map(array => array.map(message => {
        return {
          'id': message.Id,
          'content': message.Content
        } as Message;
      })),
    );
  }
}
