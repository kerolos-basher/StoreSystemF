/*
    Bags and IDs view — Domain Vw_BagsAndIDs
    IFE_Website: lookups in [Lookup] schema; registration tables in [dbo].
*/
USE [IFE_Test];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[Vw_BagsAndIDs]', N'V') IS NOT NULL
    DROP VIEW [dbo].[Vw_BagsAndIDs];
GO

CREATE VIEW [dbo].[Vw_BagsAndIDs]
AS
SELECT *
FROM dbo.RegistrationRequest RR
						outer apply(

						select U.UserName As ParticipantBagUpdatedByName
						from AspNetUsers U
						where U.Id = RR.ParticipantBagUpdatedBy

						)ParticipantBagUpdatedByName

						outer apply(

						select U.UserName As SpouseUpdatedByName
						from AspNetUsers U
						where U.Id = RR.SpouseIDUpdatedBy

						)SpouseUpdatedByName

						outer apply(

						select U.UserName As ParticipantIDUpdatedByName
						from AspNetUsers U
						where U.Id = RR.ParticipantIDUpdatedBy

						)ParticipantIDUpdatedByName


                                 cross apply(
                                   SELECT 
                                   RRT.Id AS MainRegistrationRequestTicketId
                                  , CONCAT(RRT.FirstName,' ', RRT.MiddleName ,' ',RRT.FamilyName) AS MainParticipantName
                                  , TS.EnglishDescription As MainParticipantTicketStatus
									
                                  
									, CASE
                                            WHEN RRT.TicketStatusId in(6) and RR.ParticipantID != 0
                                               THEN 'Received'
                                               ELSE  'Not Received'
                                               END as 'ParticipantIDStatus'
									, CASE
                                            WHEN RRT.TicketStatusId in(6) and RR.ParticipantBag != 0
                                               THEN 'Received'
                                               ELSE  'Not Received'
                                               END as 'ParticipantBagStatus'

                                   FROM RegistrationRequestTicket RRT
                                   join [Lookup].[LK_TicketStatus] TS on RRT.TicketStatusId = TS.Id
                                   join [Lookup].[LK_ParticipantType] PT on RRT.ParticipantTypeId = PT.Id
 
 

								 WHERE RR.Id = RRT.RegistrationRequestId 
                                   and RR.IsDeleted = 0
                                   and RRT.IsDeleted = 0
                                   and RRT.UserCategoryId = 1 
								   and RRT.TicketStatusId = 6
                                   )Main_Ticket


                                  outer apply(
                                   SELECT 
                                   RRT.Id AS SpouseRegistrationRequestTicketId
                                   ,CONCAT(RRT.FirstName,' ', RRT.MiddleName ,' ',RRT.FamilyName) AS SpouseParticipantName
                                    , TS.EnglishDescription As SpouseParticipantTicketStatus
									
									, CASE
                                            WHEN RRT.TicketStatusId in(6) and RR.SpouseID != 0
                                               THEN 'Received'
                                               ELSE  'Not Received'
                                               END as 'SpouseIDStatus'
 
 
                                   FROM  RegistrationRequestTicket RRT 
                                   join [Lookup].[LK_TicketStatus] TS on RRT.TicketStatusId = TS.Id
                                   join [Lookup].[LK_ParticipantType] PT on RRT.ParticipantTypeId = PT.Id
 
 

                                   WHERE RR.Id = RRT.RegistrationRequestId 
                                   and RR.IsDeleted = 0
                                   and RRT.IsDeleted = 0
                                   and RRT.UserCategoryId = 2
								   and RRT.TicketStatusId = 6
                                   )Spouse_Ticket
GO

