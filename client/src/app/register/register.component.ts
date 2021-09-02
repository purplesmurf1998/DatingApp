import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  model: any = {};
  confirmPassword: string = "";

  constructor() { }

  ngOnInit(): void {
  }

  register() {
    console.log(this.model);
    console.log(this.confirmPassword);
  }

  cancel() {
    console.log("Cancelled");
  }

  matchingPasswords(): boolean {
    return this.confirmPassword == this.model.password;
  }

}
