import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-main',
  standalone:false,
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent implements OnInit {

  ngOnInit(): void {
    console.log('MainComponent initialized');
  }
}
