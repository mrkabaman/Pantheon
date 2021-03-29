import { Component } from '@angular/core';
import {AccountServiceService} from './account-service.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'BankingAppUI';

  constructor(private AccountServiceService: AccountServiceService) { }

  data: any;
  AccountForm: FormGroup;
  submitted = false;
  EventValue: any = "Create";

  ngOnInit(): void {
      this.AccountForm = new FormGroup({
      accName: new FormControl('', [Validators.required]),
      accInitialDeposit: new FormControl('', [Validators.required])
    });
  }

  createAccount() {
    this.submitted = true;

    if (this.AccountForm.invalid) {
      return;
    }
    this.AccountServiceService.CreateAccount(this.AccountForm.value).subscribe((data: any[]) => {
      this.data = data;
    });
  }

  resetFrom() {
    this.AccountForm.reset();
    this.EventValue = "Create";
    this.submitted = false;
  }
}
