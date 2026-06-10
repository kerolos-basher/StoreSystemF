# IFE Website — handoff summary (for follow-up tasks)

Last updated: migration session (lookups, registration, OTP, events UI, Angular templates). Use this when continuing backend, API, or Angular work.

## Project context

| Item | Location / value |
|------|------------------|
| Solution root | `IFE_Website/` |
| API | `Apps/IFE_Api` |
| Angular app | `Apps/IFE_Website/IFEWebsite` |
| API base URL (FE) | `environment.apiUrl` → `https://localhost:7032/api` |
| Reference legacy app | `IFE_Conference/` (port logic from here; do not add separate “compat” controllers) |
| This handoff file | `Apps/IFE_Website/docs/HANDOFF.md` |

---

## Architecture conventions (current)

### Backend

| Topic | Rule |
|-------|------|
| Exceptions | `IFEException` messages from `Resources/ExceptionMessage.*` only |
| Request DTOs | `{Controller}/Request/{Name}Request.cs`, one class per file |
| Lookup schema | **All** `LK_*` entities use `[Table(..., Schema = DBSchemaKeys.Lookup)]` |
| Lookup base entity | `Domain.LookupAggregate.LookupBase` (maps `EnglishDescription` / `ArabicDescription` → `NameEn` / `NameAr`) |
| Lookup service | `ILookupService` → `Infrastructure.Services.Lookup.LookupService` (DI in `Infrastructure/Initialize.cs`) |
| **Do not use** | `Legacy*` naming, `RegistrationLookupBase`, `ILegacyLookupService`, `dbo` schema on new lookup entities |

### `DBSchemaKeys` (`Common/SystemKeys/DBSchemaKeys.cs`)

| Constant | Value | Use |
|----------|-------|-----|
| `Lookup` | `"Lookup"` | All `LK_*` lookup tables |
| `IFE` | `"dbo"` | Rare; not for new lookups |
| `Registration` | `"dbo"` | `RegistrationRequest*` tables |

**DB note:** SSMS may show tables under `dbo.LK_*`. Project maps them to schema `Lookup`. Ensure SQL schema `Lookup` exists and tables are visible there, or align `DBSchemaKeys.Lookup` with the real schema.

### Angular

| Topic | Rule |
|-------|------|
| Templates | **Use `templateUrl: './{name}.component.html'`** — no inline HTML in `.ts` |
| Components | 86+ `.component.html` files; convention changed from earlier inline-template approach |

---

## Completed work (this session)

### 1. Lookup API (ported from `IFE_Conference`)

**Service:** `Application/Abstractions/Lookup/ILookupService.cs`  
**Implementation:** `Infrastructure/Services/Lookup/LookupService.cs`  
**DTOs:** `Application/Features/LookupManagement/Contracts/LookupDtos.cs` (`LookupItemDto`, `LookupCountryDto`, …)

**Controller:** `Apps/IFE_Api/Controllers/LookupController/LookupController.cs`

- CMS lookups (MediatR): `GetUserTypes`, `GetFederationStructures`, `GetMemberCompanyCategories`, `GetPublicationTypes`, `GetRoleAsLookup`, `GetCommitteeAsLookup`
- Conference / registration lookups (`ILookupService`, `[AllowAnonymous]`):  
  `GetTitles`, `GetNationalities`, `GetCountries`, `GetCompanies`, `GetBusinessDomains`, `GetUserCategory`, `GetParticipantType`, `GetPaymentMethod`, `GetTicketType`, `GetCurrency`, `GetRegistrationRequestStatus`, `GetTicketRejectionReason`, `GetTicketsCancelationReason`, `GetCarType`, `GetFlightType`, `GetAirline`, `GetTicketRefundReason`, `GetHotel`, `GetRoomType`, `GetLanguage`, `GetTicketTypeByCompanyId`, `GetAutoCopmleteAirLine`, `ContactsCategory`, `GetSponsorshipGroup`, `GetPrivateCartypes`, `GetAllConference`, `GetConference` (POST + filter), `GetConferenceVenue`, `GetMediaCategory`, `GetEventCategory`, `GetMeetingStatus`, `GetPageModel`, `GetSendingMethod`, `GetTicketStatus`, `GetPaymentCurrencyType`

**Domain entities** (`Domain/LookupAggregate/`):

- `LookupBase`, `Title`, `Nationality`, `Country`, `Company`, `BusinessDomain`, `ConferenceVenue`, `Conference`, `Hotel`, `TicketType`, `PrivateCarType`, `ContactsCategory`, `SponsorshipGroup`
- `LookupTables.cs`: Airline, CarType, Currency, ParticipantType, RegistrationRequestStatus, TicketRejectionReason, TicketCancelationReason, TicketRefundReason, FlightType, UserCategory, RoomType, Language, PaymentMethod, MediaCategory, EventCategory, MeetingStatus, PageModel, SendingMethod, TicketStatus, PaymentCurrencyType

**DbSets:** `IFEContext` / `IApplicationDbContext` — all of the above.

**Lookup response shape (registration / website):**

```json
{ "id": 1, "nameEn": "...", "nameAr": "..." }
```

Countries add `paymentCurrencyTypeId`. Conferences use `LookupConferenceDto` (name, dates, venue names, etc.).

### 2. Registration controller lookups (FE-facing)

`RegistrationController` exposes the same data for the wizard (calls `ILookupService`):

| Endpoint | FE consumer |
|----------|-------------|
| `GET Registration/GetTitles` | `participant-details.service.ts` |
| `GET Registration/GetNationalities` | same |
| `GET Registration/GetCountries` | same |
| `GET Registration/GetCompanies` | same |
| `GET Registration/GetBusinessDomains` | same |

Also available under `api/Lookup/Get*` for legacy/admin callers.

### 3. Email check in conference

- `POST api/Registration/CheckIfEmailExistInConference` — `[AllowAnonymous]`, MediatR `CheckIfEmailExistInConferenceQuery`, `RegistrationEmailCheckService`
- Request: `CheckIfEmailExistInConferenceRequest` (`email`, `conferencId` typo kept for Angular)

### 4. OTP verification (ported from `IFE_Conference`)

- `Domain/OTPVerificationAggregate/OTPVerification.cs`
- `Infrastructure/Services/OtpVerification/OtpVerificationService.cs`
- `api/OTPVerification`: `SendEmailVerificationCode`, `ResendEmailVerificationCode`, `VerifyEmail`
- Exception keys in `ExceptionMessage.resx` (+ manual `ExceptionMessage.Designer.cs` if resx codegen lags)
- Verify accepts `emailVerificationCode` or `otpCode`; response includes `otpVerificationId`

### 5. Public events

- `Lookup/GetConferenceVenue` — `GetConferenceVenuesQuery`, entity `ConferenceVenue`
- FE `events-filter.service.ts` → `Lookup/GetConferenceVenue`
- `/events` page: red/black theme, filters, carousels; `events.component.html` + `templateUrl`

### 6. Angular: external templates

- All components use `templateUrl` + `.component.html` (not inline `template` in `.ts`)

---

## Frontend ↔ backend map

### Wired

| Area | Notes |
|------|--------|
| Auth / CMS | `Authentication`, `Lookup` (CMS + conference lookups), content controllers |
| Events (public) | `Events`, `Lookup/GetConferenceVenue` |
| Registration (partial) | Lookups, `CheckIfEmailExistInConference`, OTP; **not** full register/fees/terms yet |
| Committee / admin registration | `Committees`, `AdminRegistration` (partial) |

### Registration FE still calling missing APIs

(`registration.service.ts`)

- `Registration/GetRegistrateredUserInfo`
- `Registration/GetTicketFeesMatrix`
- `Registration/GetTicketFeesByPaymentCurrencyType`
- `Registration/GetTermsAndConditions`
- `RegisterForNewConference` (or equivalent submit)

Port from `IFE_Conference` when implementing next.

### Event dashboard stubs (unchanged)

FE still expects controllers that may not exist: `AdminNotifications`, `AdminParticipants`, `AdminLookups`, etc. — see earlier handoff list.

### Registration ID bag

- FE: `GET AdminRegistration/RegistrationIdBag` + custom update body
- API gap: no `RegistrationIdBag` GET; `UpdateParticipantID` contract mismatch

---

## Key file paths

| Area | Path |
|------|------|
| Lookup API | `Apps/IFE_Api/Controllers/LookupController/LookupController.cs` |
| Lookup service | `Infrastructure/Services/Lookup/LookupService.cs` |
| Registration API | `Apps/IFE_Api/Controllers/RegistrationController/RegistrationController.cs` |
| OTP API | `Apps/IFE_Api/Controllers/OTPVerificationController/` |
| EF context | `Infrastructure/Database/Context/IFEContext.cs` |
| Registration FE | `IFEWebsite/src/app/components/website-components/registration/` |
| Events FE | `IFEWebsite/src/app/components/website-components/events/` |
| Participant lookups FE | `.../registration/steps/participant-details/participant-details.service.ts` |

---

## Old vs new routes (reference)

| Old (`IFE_Conference`) | New (`IFE_Website`) |
|------------------------|------------------------|
| `Lookup/GetTitles` etc. | `Lookup/Get*` **and** `Registration/Get*` (wizard) |
| `Lookup/GetConferenceVenue` | `Lookup/GetConferenceVenue` |
| `OTPVerification/SendEmailVerificationCode?conferenceId=` | Same |
| `api/CheckIfEmailExistInConference` | `Registration/CheckIfEmailExistInConference` only (no root compat controller) |

---

## Build & run

```bash
dotnet build IFE_Website/Apps/IFE_Api/IFE_Api.csproj
cd IFE_Website/Apps/IFE_Website/IFEWebsite && npm run build
```

**API:** Restart `IFE_Api` after backend changes (VS often locks DLLs).

**Test lookups:**

- `https://localhost:7032/api/Registration/GetCompanies`
- `https://localhost:7032/api/Lookup/GetCompanies`

---

## Suggested next tasks

1. Port remaining **registration flow** from `IFE_Conference` (fees matrix, terms, `GetRegistrateredUserInfo`, submit registration).
2. Implement or stub **event-dashboard** `Admin*` endpoints.
3. **RegistrationIdBag** GET + update contract on `AdminRegistration`.
4. Confirm **SQL schema `Lookup`** matches EF mapping (or fix `DBSchemaKeys.Lookup`).
5. New `IFEException` → `ExceptionMessage` resx + Designer.
6. New Angular components → **`templateUrl` + `.html`**, not inline templates.
7. New API requests → `{Controller}/Request/` folder.

---

## Event dashboard — admin Excel exports (registration / accommodation)

Multi-type export (`GetPopUpExcelData`, `GetAccommodationExcelData`) reads the **same SQL views** as the legacy portal:

- `dbo.Vw_PopUpExcelViews` — Registration, Transportation, Payment & Finance (same dataset; columns chosen on the client).
- `dbo.Vw_Accommodation` — Accommodation export (legacy also forces main user category in `RegistrationRequestAccommodationController` before querying).

Filtering is ported from `IFE_Conference` `RegistrationRequestRepository.GetPopUpExcelQuery` / `GetAccommodationExcelQuery`. **Do not** rebuild this dataset by composing EF aggregates unless the views are removed from the database.

Ensure these views exist in the deployed DB (migrated from `IFE_Conference` or equivalent). Login report and session rating exports are still pending unless `UserLoginLog` / event rate sources are added to `IFE_Website`.

---

## Rules checklist for next agent

- [ ] No `Legacy*` types or filenames in new code
- [ ] Lookup entities: `Schema = DBSchemaKeys.Lookup`, inherit `LookupBase` where applicable
- [ ] Use `ILookupService` for lookup queries (don’t duplicate in controllers)
- [ ] Port from `IFE_Conference`, wire on existing controllers (avoid duplicate compat routes unless user asks)
- [ ] Angular: external HTML templates only
- [ ] `IFEException` → resources only
- [ ] Restart API after build when testing endpoints
