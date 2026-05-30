# Registration SQL views

Scripts create views that match:

| SQL view | Domain entity | Angular / API |
|----------|---------------|---------------|
| `Vw_PopUpExcelViews` | `Domain/RegistrationRequestAggregate/Views/Vw_PopUpExcelViews.cs` | `IRegistrationPopUpExcelRow` ← `RegistrationPopUpExcelRowDto` |
| `Vw_Accommodation` | `Domain/RegistrationRequestAggregate/Views/Vw_Accommodation.cs` | Same DTO; includes `AccommodationComment` |
| `Vw_BagsAndIDs` | `Domain/RegistrationRequestAggregate/Views/Vw_BagsAndIDs.cs` | Bags/IDs export (when wired) |

Each script uses **DROP VIEW IF EXISTS** then **CREATE VIEW** (not `ALTER`).

Column alias note: flight apply exposes `RF.AirlineId AS AirLineId` to match the C# property `AirLineId`.

## Deploy

1. Set `USE [IFE_Test]` to your database name in each `.sql` file.
2. Run in order (SSMS or `sqlcmd`):

```bat
sqlcmd -S YOUR_SERVER -d IFE_Test -i Vw_PopUpExcelViews.sql
sqlcmd -S YOUR_SERVER -d IFE_Test -i Vw_Accommodation.sql
sqlcmd -S YOUR_SERVER -d IFE_Test -i Vw_BagsAndIDs.sql
```

Requires existing tables:

- **dbo**: `RegistrationRequest`, `RegistrationRequestTicket`, `RegistrationRequestFlight`, `RegistrationRequestAccommodation`, `AspNetUsers`
- **Lookup**: `LK_Country`, `LK_Currency`, `LK_Company`, etc. (IFE_Website uses schema `Lookup`, not `dbo`)

Lookup description column is `EnglishDescription` (same as legacy portal scripts).

### IFE_Website column mapping (view aliases legacy names for Domain/EF)

| Legacy view column | IFE_Website source |
|--------------------|-------------------|
| `UserCategory` (from users) | `AspNetUsers.UserTypeId` → `LK_UserCategory` |
| `CairoInternationalAirport` | `RegistrationRequestFlight.CairoArrivalNumber` |
| `FlightArrivalDateCairo` | `CairoArrivalDate` |
| `FlightArrivalLuxorNo` | `LuxorArrivalNumber` |
| `FlightArrivalDateLuxor` | `LuxorArrivalDate` |
| `FlightArrivalTimeLuxor` | `LuxorArrivalTime` |
| `FlightArrivalTimeCairo` | `CairoArrivalTime` |
| `FlightDepartureNo/Date/Time` | `CairoDepartureNumber/Date/Time` |
| Ticket `UserCategoryId` filter | `RegistrationRequestTicket.UserCategoryId` (unchanged) |
