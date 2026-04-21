# Short Description for the application

This is application is focused on the retail sector specifically for real estate, where people can find, purchase and live in a new home, whether it be a flat, apartment or 1-2 story housing etc. It will also include extra security features. And cookies to assist in authentication for the login and registration. Users can also search for any accommodation that they prefer based on their preferences, same with searching/ filtering. Once a user purchases an property an email should be sent to them, users should also be able to book a viewing for properties that are available for viewing.

This idea includes the following features:

    Login/Registration
	    Users and Admins
            Users – Browses Website, looking for accommodations (flats, apartments, houses etc.) - READ only
            Admin – Is able to add, delete, view and update, accommodations, users as well as booked viewings (Create, Read, Update & Delete) - Full CRUD
               - Excluding Create, Update and Delete for the transactions
    Searching & Filtering which accommodations the user wants by number of pets, bedrooms/bathrooms, price etc.
        Detailed description shows up when the user views one of the available accommodation options
    Viewing
        Users can see what houses are available for viewing
    Transaction & Checkout systems
        Purchasing an available accommodation
        Email Notifications


Advanced features will also be:

    Cookie Authentication (Implemented before it was covered)
    Extra Security features

Classes involved:

    Users
    Admin
    Property (Houses, Apartments, Flats etc.)
    Viewing
    Transactions
    CheckoutViewModel

Properties involved:

    UserID, User First Name, User Last Name, Email, UserName, Password
    Admin UserName & Password
    PropertyID, Property Name, Property Address, Property Bedrooms, Property Bathrooms, Property Pets & Property Garage(s), Extended Description, IsAvailable, Nearby Schools, Nearby Shops
    ViewingID, PropertyID, UserID, Viewing_TimeDate
    TransactionID, PropertyID, UserID, Price, UserEmail, Buyer Name, Purchase Date
    PropertyID, Property Name, Property Address, Price, Buyer Name, UserEmail, Phone Number, Billing Address, Card Number, Card holder name, Expiry Date, CVV




Justin:

    Property Model + CRUD
    Searching and Filtering logic
    Transaction and Checkout system
    Email notification on purchase
    Extra Security Features

Ravi:

    User + Admin models + Login/Registration
    Role-based access (User = read-only, Admin = Full CRUD - except transactions, transactions is Read only)
    Viewing model
    Cookie Authentication

Advanced features can be done together or by whoever finishes their section first
