import { Http, Response, Headers, RequestOptions } from "@angular/http";
import { Injectable, OnInit } from "@angular/core";

import { Observable } from "rxjs/observable";
import "rxjs/add/operator/catch";
import "rxjs/add/operator/map";
import { UserFiles } from "../models/UserFiles"

@Injectable()
export class FilesService implements OnInit{
    /**
     *
     */
    constructor(private http: Http) {
        
        
    }
    private apiUrl = "https://cloudstor.azurewebsites.net/api/";
    private appKey = "?code=EhFRyFnfijUwGVjODoT8MA6GThywwfPG6A0sy/yQDlga5gEMmcxmFw==";
    getFilesForUser(username:string):Observable<Array<UserFiles>>{
        let headers = new Headers({ 'Content-Type':'application/json'});
        let options = new RequestOptions({ headers: headers });
        let apiUrl = this.apiUrl + username + "/files/" + this.appKey;
        return this.http.get(apiUrl)
                        .map(res => res.json())//.map(this.extractData)
                        .catch(this.handleError);//, {username}, options)
    }
    ngOnInit() {
        this.appKey = "?code=EhFRyFnfijUwGVjODoT8MA6GThywwfPG6A0sy/yQDlga5gEMmcxmFw==";        
        //throw new Error("Method not implemented.");
    }

    private extractData(response: Response){
        let body = response.json();
        return body.data || { };
    }

    private handleError(error: Response | any):Observable<any>{
        let errMessage:string;
        if(error instanceof Response){
            const body = error.json() || "";
            const err = body.error || JSON.stringify(body);
            errMessage = `${error.status} - ${error.statusText || ''} ${err}`;
        }else {
            errMessage = error.message ? error.message : error.toString();
        }
        console.error(errMessage);
        return Observable.throw(errMessage);
    }
}