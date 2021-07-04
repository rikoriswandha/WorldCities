import { AuthorizeGuard } from "./../api-authorization/authorize.guard";
import { CityEditComponent } from "./cities/city-edit.component";
import { CountriesComponent } from "./countries/countries.component";
import { CitiesComponent } from "./cities/cities.component";
import { HomeComponent } from "./home/home.component";
import { RouterModule, Routes } from "@angular/router";
import { NgModule } from "@angular/core";
import { CountryEditComponent } from "./countries/country-edit.component";

const routes: Routes = [
  { path: "", component: HomeComponent, pathMatch: "full" },
  { path: "cities", component: CitiesComponent },
  {
    path: "city/:id",
    component: CityEditComponent,
    canActivate: [AuthorizeGuard],
  },
  { path: "city", component: CityEditComponent, canActivate: [AuthorizeGuard] },
  { path: "countries", component: CountriesComponent },
  {
    path: "country/:id",
    component: CountryEditComponent,
    canActivate: [AuthorizeGuard],
  },
  {
    path: "country",
    component: CountryEditComponent,
    canActivate: [AuthorizeGuard],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
