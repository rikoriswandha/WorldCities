import { Observable } from "rxjs";
import { ApiResult } from "./../base.service";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { BaseService } from "../base.service";

@Injectable({
  providedIn: "root",
})
export class CityService extends BaseService {
  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    super(http, baseUrl);
  }

  getData<ApiResult>(
    pageIndex: number,
    pageSize: number,
    sortColumn: string,
    sortOrder: string,
    filterColumn: string,
    filterQuery: string
  ): Observable<ApiResult> {
    var url = this.baseUrl + "api/cities";
    var params = new HttpParams()
      .set("pageIndex", pageIndex.toString())
      .set("pageSize", pageSize.toString())
      .set("sortColumn", sortColumn)
      .set("sortOrder", sortOrder);

    if (filterQuery) {
      params = params
        .set("filterColumn", filterColumn)
        .set("filterQuery", filterQuery);
    }

    return this.http.get<ApiResult>(url, { params });
  }

  get<City>(id): Observable<City> {
    var url = this.baseUrl + "api/cities/" + id;
    return this.http.get<City>(url);
  }

  put<City>(item): Observable<City> {
    var url = this.baseUrl + "api/cities/" + item.id;
    return this.http.put<City>(url, item);
  }

  post<City>(item): Observable<City> {
    var url = this.baseUrl + "api/cities";
    return this.http.post<City>(url, item);
  }

  getCountries<ApiResult>(
    pageIndex: number,
    pageSize: number,
    sortColumn: string,
    sortOrder: string,
    filterColumn: string,
    filterQuery: string
  ): Observable<ApiResult> {
    var url = this.baseUrl + "api/countries";
    var params = new HttpParams()
      .set("pageIndex", pageIndex.toString())
      .set("pageSize", pageSize.toString())
      .set("sortColumn", sortColumn)
      .set("sortOrder", sortOrder);

    if (filterQuery) {
      params = params
        .set("filterColumn", filterColumn)
        .set("filterQuery", filterQuery);
    }

    return this.http.get<ApiResult>(url, { params });
  }

  isDupeCity(item): Observable<boolean> {
    var url = this.baseUrl + "api/cities/isdupecity";
    return this.http.post<boolean>(url, item);
  }
}
