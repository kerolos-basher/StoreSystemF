/*
    PopUp Excel view — Domain Vw_PopUpExcelViews + IRegistrationPopUpExcelRow
    IFE_Website: lookups in [Lookup] schema; registration tables in [dbo].
*/
USE [IFE_Test];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[Vw_PopUpExcelViews]', N'V') IS NOT NULL
    DROP VIEW [dbo].[Vw_PopUpExcelViews];
GO

CREATE VIEW [dbo].[Vw_PopUpExcelViews]
AS
SELECT *
    , CASE
                                               WHEN RR.IsCarPaid = null 
                                               THEN null
                                               WHEN RR.IsCarPaid = 1  
											   THEN 'Paid'
											    WHEN RR.IsCarPaid = 0  
											   THEN 'Not Paid'
                                               END AS RentedCarPaymentStatus
FROM dbo.RegistrationRequest RR
     									  cross APPLY 
     									  (   
     									  SELECT Country.EnglishDescription As Country 
     									  from [Lookup].[LK_Country] Country
     									  where Country.Id = RR.CountryId
     									  ) country

										outer APPLY 
     									  (   
     									  SELECT PrivateCar.EnglishDescription As PrivateCar 
     									  from [Lookup].[LK_PrivateCarType] PrivateCar
     									  where PrivateCar.Id = RR.CarTypeId
     									  ) PrivateCar

										cross APPLY 
     									  (   
     									  SELECT Currency.EnglishDescription As Currency 
     									  from [Lookup].[LK_Currency] Currency
     									  where Currency.Id = RR.PaymentCurrencyTypeId
     									  ) Currency

										cross APPLY 
     									  (   
     									  SELECT distinct Category.EnglishDescription As UserCategory 
     									  from AspNetUsers ASP
                                  		  join [Lookup].[LK_UserCategory] Category on Category.Id =ASP.UserTypeId
     									  where  RR.ApplicationUserId = ASP.Id
										  and ASP.IsDeleted = 0
     									  ) Category

     									  outer apply(
                                   SELECT distinct
										RF.Id As FlightId,
										RF.AirlineId AS AirLineId,
                                        RF.CairoArrivalNumber AS CairoInternationalAirport
                                        , RF.CairoArrivalDate AS FlightArrivalDateCairo 
                                        , RF.LuxorArrivalNumber AS FlightArrivalLuxorNo 
                                        , RF.OtherAirlineName
                                        , RF.LuxorArrivalDate AS FlightArrivalDateLuxor
                                        , RF.LuxorArrivalTime AS FlightArrivalTimeLuxor
                                        , RF.CairoDepartureNumber AS FlightDepartureNo
                                        , RF.CairoDepartureDate AS FlightDepartureDate
                                        , RF.CairoDepartureTime AS FlightDepartureTime
                                        , A.EnglishDescription As Airline
                                        , CT.EnglishDescription AS CarType
                                        , F.EnglishDescription As FlightType
                                        , RF.NumberOfTickets
                                        ,RF.Comment
										, CASE
                                            WHEN RF.RegistrationRequestId in(RR.Id)
                                               THEN 'Yes'
                                               ELSE  'No'
                                               END as 'FlightReservation'
							
 
 
                                   FROM RegistrationRequestFlight RF
                                   left join [Lookup].[LK_Airline] A on A.Id = RF.AirlineId
                                   left join [Lookup].[LK_FlightType] F    on F.Id = RF.FlightTypeId
                                    left join [Lookup].[LK_CarType] CT on CT.Id = RF.CarTypeId
 
 
                                   WHERE RR.Id = RF.RegistrationRequestId 
                                   and RR.IsDeleted = 0
                                   and RF.IsDeleted = 0
                                   )Flight
 
                                 outer apply(
                                   SELECT 
                                   RRT.Id AS MainRegistrationRequestTicketId
                                  , CONCAT(RRT.FirstName,' ', RRT.MiddleName ,' ',RRT.FamilyName) AS MainParticipantName
                                    , RRT.Email As MainEmail
                                    , RRT.Mobile As MainMobile
                                     ,RRT.Discount As MainDiscount
                                    , RRT.Position As MainPosition
                                    , N.EnglishDescription AS MainNationality
                                    , RRT.ParticipantPrintRecipt AS MainParticipantPrintRecipt
									 , RRT.RefundedTicketAmount As MainRefundedAmount
                                    , TT.EnglishDescription AS MainRegestrationType
                                    , PT.EnglishDescription As MainParticipantType
                                    , TS.EnglishDescription As MainParticipantTicketStatus
									, RRT.TicketFees As ParticipantFess
									, RRT.TicketFeesAfterDiscount As ParticipantFessAfterDiscount

									, CASE
                                            WHEN RRT.CompanyId = 1
                                               THEN RRT.OtherCompanyName
                                               ELSE C.EnglishDescription
                                               END as 'MainCompany'
									
                                    , CASE
                                            WHEN RRT.TicketStatusId in(6,7)
                                               THEN 'Paid'
                                               ELSE 'Not Paid'
                                               END as 'Participant_PaidORUnpaid'
									, CASE
                                            WHEN RRT.TicketStatusId in(6,7) and RR.ParticipantID != null
                                               THEN 'Yes'
                                               ELSE  'No'
                                               END as 'ParticipantID_Flag'
									, CASE
                                            WHEN RRT.TicketStatusId in(6,7) and RR.ParticipantBag != null
                                               THEN 'Yes'
                                               ELSE  'No'
                                               END as 'ParticipantBag_Flag'
                                   FROM RegistrationRequestTicket RRT
 
 
                                 left  join [Lookup].[LK_Company] C on C.Id = RRT.CompanyId
                                  left join [Lookup].[LK_Nationality] N on RRT.NationalityId = N.Id
                                 left join [Lookup].[LK_TicketType] TT on RRT.TicketTypeId = TT.Id
                                  left join [Lookup].[LK_ParticipantType] PT on RRT.ParticipantTypeId = PT.Id
                                  left join [Lookup].[LK_TicketStatus] TS on RRT.TicketStatusId = TS.Id
                               
 

								 WHERE RR.Id = RRT.RegistrationRequestId 
                                   and RR.IsDeleted = 0
                                   and RRT.IsDeleted = 0
                                   and RRT.UserCategoryId = 1 
                                   )Main_Ticket


                                 outer apply(
                                   SELECT 
                                   RRT.Id AS SpouseRegistrationRequestTicketId
                                   ,CONCAT(RRT.FirstName,' ', RRT.MiddleName ,' ',RRT.FamilyName) AS SpouseParticipantName
                                    , RRT.Email As SpouseEmail
                                    , RRT.Mobile As SpouseMobile
                                    , RRT.Position As SpousePosition
                                    ,RRT.Discount As SpouseDiscount
                                    , N.EnglishDescription AS SpouseNationality
                                    , RRT.ParticipantPrintRecipt AS SpouseParticipantPrintRecipt
									, RRT.RefundedTicketAmount As SpouseRefundedAmount
                                    , TT.EnglishDescription AS SpouseRegestrationType
                                    , PT.EnglishDescription As SpouseParticipantType
                                    , TS.EnglishDescription As SpouseParticipantTicketStatus
									, RRT.TicketFees As SpouseFess
									, RRT.TicketFeesAfterDiscount As SpouseFessAfterDiscount
									
									 , CASE
                                            WHEN RRT.CompanyId = 1
                                               THEN RRT.OtherCompanyName
                                               ELSE C.EnglishDescription
                                               END as 'SpouseCompany'

                                    , CASE
                                            WHEN RRT.TicketStatusId in(6,7)
                                               THEN 'Paid'
                                               ELSE 'Not Paid'
                                               END as 'SpouseParticipant_PaidORUnpaid'
									

									, CASE
                                            WHEN RRT.TicketStatusId in(6,7) and RR.SpouseID != null
                                               THEN 'yes'
                                               ELSE  'No'
                                               END as 'SpouseID_Flag'
 
 
                                   FROM RegistrationRequestTicket RRT
 
 
                                  left join [Lookup].[LK_Company] C on C.Id = RRT.CompanyId
                                  left join [Lookup].[LK_Nationality] N on RRT.NationalityId = N.Id 
                                  left join [Lookup].[LK_TicketType] TT on RRT.TicketTypeId = TT.Id
                                  left join [Lookup].[LK_TicketStatus] TS on RRT.TicketStatusId = TS.Id
                                  left join [Lookup].[LK_ParticipantType] PT on RRT.ParticipantTypeId = PT.Id
 
 

                                   WHERE RR.Id = RRT.RegistrationRequestId 
                                   and RR.IsDeleted = 0
                                   and RRT.IsDeleted = 0
                                   and RRT.UserCategoryId = 2
                                   )Spouse_Ticket

	outer APPLY 
                                   ( 
                                   SELECT top 1
                                              RA.CheckInDate As MainCheckInDate
                                             , RA.CheckOutDate As MainCheckOutDate
                                             , H.EnglishDescription As MainHotel
                                             ,R.EnglishDescription As RoomType
                                             ,RR1.RegistrationCode as RegistrationCodeOne
											 ,RR2.RegistrationCode as RegistrationCodeTwo
											 ,RR3.RegistrationCode as RegistrationCodeThree	
                                             ,CONCAT(RT.FirstName,' ', RT.MiddleName ,' ',RT.FamilyName)  AS ResidentOne
                                             ,CONCAT(RT2.FirstName,' ', RT2.MiddleName ,' ',RT2.FamilyName)  AS ResidentTwo
                                             ,CONCAT(RT3.FirstName,' ', RT3.MiddleName ,' ',RT3.FamilyName)  AS ResidentThree
                                   FROM RegistrationRequestAccommodation RA
								   
                                   left join RegistrationRequestTicket RT on RT.Id = RA.FirstParticipantTicketId
                                   left join RegistrationRequestTicket RT2 on RT2.Id = RA.SecondParticipantTicketId
                                   left join RegistrationRequestTicket RT3 on RT3.Id = RA.ThirdParticipantTicketId

								   left join RegistrationRequest RR1 on RR1.Id = RT.RegistrationRequestId
                                   left join RegistrationRequest RR2 on RR2.Id = RT2.RegistrationRequestId
                                   left join RegistrationRequest RR3 on RR3.Id = RT3.RegistrationRequestId

                                   left   join [Lookup].[LK_Hotel] H on RA.HotelId = H.Id
                                   left   join [Lookup].[LK_RoomType] R on R.Id = RA.RoomTypeId
                                   WHERE RR.Id = RA.RegistrationRequestId 
								
                                   and RR.IsDeleted = 0
                                   and RA.IsDeleted = 0
                                   )Accommodation
GO

