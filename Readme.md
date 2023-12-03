# Question 08
Web Application Development using Clean Architecture

Read User Story below carefully:

The business unit require a web application to receive document (xlsx and pdf) from customer and store those documents into database. Before send the required documents customer should be register if not registered yet. System should be authenticated the user as customer before send documents to the system. In Password should be fulfilled with requirements as stated below:

•It contains at least one lowercase English character. 
•It contains at least one uppercase English character. 
•It contains at least one special character. The special characters are: !@#$%^&*()-+ 
•Its length is at least 8. 
•It contains at least one digit. 

When documents store successfully in database (transaction completely), system send notification to customer as receipt that document is submitted successfully. The size of Documents are more than 1 or 2 GBs, and technically required handling like chunking method.

The business unit able to monitor the documents which sent by customers and unit business able to download those documents.

Task: Develop web application using clean architecture web API .net core 7, and entity framework using c#.

# Analysis

Analysis breakdown of task that need to be completed from story
## 01. User Registration and Authentication
- allow user registration
- implement password validation for user creation to ensure passwords meet the specified criteria
- implement authentication system to verify user's identity before allowing document submission
## 02. Document Submission
- Customer type user can submit document
- Implement document validation for file type (xlsx and pdf)
- Use a chunking method to handle large file uploads
- Store the documents in a database
- Integrate a notification system to send a receipt to the customer upon successful document submission
## 03. Document Retrieval
- Document listing
- Document download feature
	- only able to download as business unit user (this will be assumed as the use case, need confirm though)
	- download document by id
	- download all document by customer id

# Implementation
Implementation checklist
- [x] allow user registration
- [x] implement password validation for user creation to ensure passwords meet the specified criteria
- [x] implement authentication system to verify user's identity before allowing document submission
- [x] Customer type user can submit document
- [x] Implement document validation for file type (xlsx and pdf)
- [ ] Use a chunking method to handle large file uploads
- [x] Store the documents in a database
- [ ] Integrate a notification system to send a receipt to the customer upon successful document submission
- [x] Document listing
- [x] only able to download after verified as business unit user only
- [x] download document by id
- [x] download all document by customer id

Some change info on un-implemented feature
- Database store for documents
	- Though database can be use as data storage (using FILESTREAM in the case of SQL server), it is not advised to do so. Instead, we save file name and path for uploaded files in database while the files it self are on disk / filesystem.
	- saving files on disk will make it easier to manage, or spread on distributed storage. this will also keep the database slimmer and greatly increase the speed of backup/recovery.
- Document submission still upload as single file, albeit adding limit of 2GB file size.
	- chunks upload in on todo list
	- this can be implemented by creating 2 endpoints on the backend side to handle file in server side, and script on frontend side to split segments of the file / document to be fed to backend endpoint.
		- endpoint to upload file with parameter like filename, chunk order, chunk byte. this will store those chunks in temporary path in server
		- endpoint to merge the chunk by giving filename parameter. front end will signal this endpoint after completing last chunk upload, so backend will merge the file and save final path to database.
		
- Notification receipt for upload
	- API endpoint for document submission gave response for id and filename. Upload receipt should be able to be implemented in Frontend. unless by "notification system" meant to sent email / message to customer then it would need to add additional API for those notification. This case need more clarification from user story.