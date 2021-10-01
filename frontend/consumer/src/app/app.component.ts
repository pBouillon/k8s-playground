import { Component, OnDestroy, OnInit } from '@angular/core';
import { EMPTY, interval, Observable, Subject } from 'rxjs';
import { retry, switchMap, takeUntil } from 'rxjs/operators';
import { Message } from './message';
import { MessageService } from './message.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  
  private _stopPolling = new Subject();

  retrievedByPolling$: Observable<Message[]> = EMPTY;
  retrievedFromQueue$: Observable<Message[]> = EMPTY;

  constructor(
    private readonly _messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.retrievedByPolling$ = interval(1_000).pipe(
      switchMap(_tick => this._messageService.getMessages()),
      retry(),
      takeUntil(this._stopPolling),
    );

    this.retrievedFromQueue$ = interval(1_000).pipe(
      switchMap(_tick => this._messageService.getMessagesFromConsumerApi()),
      retry(),
      takeUntil(this._stopPolling),
    );
  }

  ngOnDestroy(): void {
    this._stopPolling.next();
  }

}
