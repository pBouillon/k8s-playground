import { Component, Input, OnInit } from '@angular/core';
import { Message } from '../message';

@Component({
  selector: 'app-alert',
  templateUrl: './alert.component.html',
  styleUrls: ['./alert.component.css']
})
export class AlertComponent implements OnInit {

  @Input()
  message!: Message;

  constructor() { }

  ngOnInit(): void {
  }

}
