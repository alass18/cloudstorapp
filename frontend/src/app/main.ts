import {bootstrap} from '@angular/platform-browser-dynamic';
import {ROUTER_DIRECTIVES, ROUTER_PROVIDERS} from '@angular/router-deprecated';
import {AdalService} from 'angular2-adal/core';
import {AppComponent} from './components/app.component';
import { FilesService } from './services/files.service';
import {SecretService} from './services/secret.service';
import { Http, Jsonp, HTTP_PROVIDERS, HTTP_BINDINGS } from '@angular/http'

bootstrap(AppComponent, [AdalService, SecretService, FilesService, Http, Jsonp, HTTP_PROVIDERS, HTTP_BINDINGS, ROUTER_PROVIDERS, ROUTER_DIRECTIVES]);