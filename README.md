# SmartScanner
CPE Business Hackathon
## What
The app scans business/lead information from the business cards, extracts the useful lead information  from it and  creates a "Lead" in Dynamics CRM  in real time. 
This information is then available to Ops/Sales team for review and sales.

## Why
Manually typing new leads from business cards is tedious and time consuming. It requires the user to familiar with Dynamics Environment  and have the resources to do it. 
Sometimes sales guys missed few leads, also missed to pass important initial customer info (customer category like fortune 500, position in the company etc.). 
Dynamics 365 Smart Lead Scanner is a portable and automated solution to this problem.

## How
A mobile app will capture the business card details as an image. App will send the Image to a web API. Web API in the background will call Azure Vision API which will decode the Image to Text and add regular expressions to distinguish between Name, Address, Company, Phone No, Email etc. Also Web API try to fetch customer imp details like category, credit score etc.
Finally it will update the Leads & Customer Insight entities
