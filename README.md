# KdajBi
 Yet another the best appointment application/appointment scheduler/appointment system

See it in action: [KdajBi.si](https://KdajBi.si) (Page in Slovenian)

~~Plan is to have~~ We have:
1. Backend (core 6 webapp) and ~~frontend (vue.js)~~ [adminlte.io](https://adminlte.io) frontend for managing and accepting appointments. 
2. User authentication via Google. 
3. Google Calendar for storing appointments.
4. SqlServer for storing all other data.
5. Separate API for data access.

Done functionalities:
1. Google authentication
2. New user registration
3. Locations, employees, workplaces, schedules (fixed and odd/even weeks, exceptions)
4. Services (used for making appointments)
5. Clients records
6. Lock/unlock admin mode
7. SMS client notifications (sms messagaging is not part of this repo, since it is done trough local sms provider)
8. Making appointments
9. Sending a private booking link to a client 
10. Public link for clients to book appointments
