import { Component, OnDestroy, OnInit } from "@angular/core";
import {
  AbstractControl,
  AsyncValidatorFn,
  FormControl,
  FormGroup,
  Validators,
} from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { Observable, Subject, Subscription } from "rxjs";
import { map, takeUntil } from "rxjs/operators";
import { BaseFormComponent } from "../base.form.component";
import { ApiResult } from "../base.service";
import { Country } from "../countries/country";
import { City } from "./city";
import { CityService } from "./city.service";

@Component({
  selector: "app-city-edit",
  templateUrl: "./city-edit.component.html",
  styleUrls: ["./city-edit.component.css"],
})
export class CityEditComponent
  extends BaseFormComponent
  implements OnInit, OnDestroy
{
  title: string;
  form: FormGroup;
  city: City;
  id?: number;
  countries: Observable<ApiResult<Country>>;
  private subscriptions: Subscription = new Subscription();
  private destroySubject: Subject<boolean> = new Subject<boolean>();

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cityService: CityService
  ) {
    super();
  }

  ngOnInit() {
    this.form = new FormGroup(
      {
        name: new FormControl("", Validators.required),
        lat: new FormControl("", [
          Validators.required,
          Validators.pattern(/^[-]?[0-9]+(\.[0-9]{1,4})?$/),
        ]),
        lon: new FormControl("", [
          Validators.required,
          Validators.pattern(/^[-]?[0-9]+(\.[0-9]{1,4})?$/),
        ]),
        countryId: new FormControl("", Validators.required),
      },
      null,
      this.isDupeCity()
    );

    // React to form changes
    this.subscriptions.add(
      this.form.valueChanges
        .pipe(takeUntil(this.destroySubject))
        .subscribe(() => {
          if (!this.form.dirty) {
            this.log("Form Model has been loaded.");
          } else {
            this.log("Form was updated by the user.");
          }
        })
    );

    // React to changes in the form.name control
    this.subscriptions.add(
      this.form
        .get("name")
        .valueChanges.pipe(takeUntil(this.destroySubject))
        .subscribe(() => {
          if (!this.form.dirty) {
            this.log("Name has been loaded with initial value.");
          } else {
            this.log("Name was updated by the user.");
          }
        })
    );

    this.loadData();
  }

  ngOnDestroy() {
    // Emit a value with the takeUntil notifier
    this.destroySubject.next(true);
    // Unsubscribe from the modifier
    this.destroySubject.unsubscribe();
  }

  log(str: string) {
    console.log(`[${new Date().toLocaleString()}] ${str}`);
  }

  loadData() {
    this.loadCountries();

    this.id = +this.activatedRoute.snapshot.paramMap.get("id");

    if (this.id) {
      // EDIT MODE

      this.cityService.get<City>(this.id).subscribe(
        (result) => {
          this.city = result;
          this.title = "Edit - " + this.city.name;

          this.form.patchValue(this.city);
        },
        (error) => console.error(error)
      );
    } else {
      this.title = "Create a new City";
    }
  }

  loadCountries() {
    // this.cityService
    //   .getCountries<ApiResult<Country>>(0, 9999, "name", null, null, null)
    //   .subscribe(
    //     (result) => {
    //       this.countries = result.data;
    //     },
    //     (error) => console.error(error)
    //   );
    this.countries = this.cityService.getCountries<ApiResult<Country>>(
      0,
      9999,
      "name",
      null,
      null,
      null
    );
  }

  onSubmit() {
    var city = this.id ? this.city : <City>{};

    city.name = this.form.get("name").value;
    city.lat = +this.form.get("lat").value;
    city.lon = +this.form.get("lon").value;
    city.countryId = +this.form.get("countryId").value;

    if (this.id) {
      // EDIT MODE

      this.cityService.put<City>(city).subscribe(
        (result) => {
          console.log(`City ${result.id} has been updated`);

          this.router.navigate(["/cities"]);
        },
        (error) => console.error(error)
      );
    } else {
      this.cityService.post<City>(city).subscribe(
        (result) => {
          console.log(`City ${city.id} has been created`);

          this.router.navigate(["/cities"]);
        },
        (error) => console.error(error)
      );
    }
  }

  isDupeCity(): AsyncValidatorFn {
    return (
      control: AbstractControl
    ): Observable<{ [key: string]: any } | null> => {
      var city = <City>{};
      city.id = this.id ? this.id : 0;
      city.name = this.form.get("name").value;
      city.lat = +this.form.get("lat").value;
      city.lon = +this.form.get("lon").value;
      city.countryId = +this.form.get("countryId").value;

      console.log(city);

      return this.cityService.isDupeCity(city).pipe(
        map((result) => {
          return result ? { isDupeCity: true } : null;
        })
      );
    };
  }
}
