import {Component, OnInit} from '@angular/core';
import {AdalService} from 'angular2-adal/core';
import {ProtectedDirective} from "../../directives/protected.directive";
import {FilesService} from "../../services/files.service";
import {UserFiles} from "../../models/UserFiles";
import {Observable} from "rxjs/observable";

@Component({
    selector: 'home',
    directives: [ProtectedDirective],
    templateUrl: './app/components/home/home-view.html'
})
export class HomeComponent implements OnInit {
    private files: Array<UserFiles>;
    constructor(
        private adalService: AdalService,
        private filesService: FilesService
    ) {
        console.log('Entering home');
        this.files = [];
    }

    ngOnInit() {
        this.getItems();
        //console.log(this.files);
    }
    public logOut() {
        this.adalService.logOut();
    }

    public getItems(){
        this.filesService
            .getFilesForUser("mouad")
            .subscribe(data => this.files = data);
        console.log(this.files);
    }
}
