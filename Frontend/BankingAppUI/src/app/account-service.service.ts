import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
@Injectable({
  providedIn: 'root'
})

@Injectable({
  providedIn: 'root'
})
export class AccountServiceService {

  private accessPointUrl = 'https://localhost:5001';

  constructor(private http: HttpClient) { }
  httpOptions = {
    headers: new HttpHeaders({'Content-Type': 'application/json'})
  };

  CreateAccount(formData) {
    return this.http.post(this.accessPointUrl + '/account/create', formData);
  }

  CreateDeposit(formData) {
    return this.http.post(this.accessPointUrl + '/account/deposit', formData);
  }

  CreateWithdrawal(formData) {
    return this.http.post(this.accessPointUrl + '/account/withdraw', formData);
  }

  GetBalance(id) {
    return this.http.get(this.accessPointUrl + '/account/balance/' + id);
  }

  GetTransactions(id) {
    return this.http.get(this.accessPointUrl + '/account/transactions/' + id);
  }

}
