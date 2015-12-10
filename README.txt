Setup

Solution was compiled using Microsoft Visual Studio 2013 for Web



Technologies Used

ASP.NET Web API - create web service

Entity Framework - handle database access

Web API Help Page - generates documentation for API

Web API Test Client - allows easy testing from API Help pages

Moq - mocking objects for testing


Data Model

Refer to datamodel.png for the data model

The Claims and Vehicles have a many-to-many relationship since a claim can have multiple vehicles involved, and I assumed that a vehicle can be listed under multiple claims.

