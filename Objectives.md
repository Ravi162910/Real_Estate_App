# Short Description for the application

This is application is focused on the retail sector, where people can find, purchase and live in a new home, whether it be a flat, apartment or 1-2 story housing. It will also include extra security features and a chatbot that increases user interactivity within the app, as users get support from this chatbot whenever they get stuck on navigation or bugs. Users can also search for any accommodation that they prefer based on their preferences, same with filtering. Once a user purchases a property an email should be sent to them, users should also be able to view properties that are available for viewing.

This idea includes the following features:

    Login/Registration
    -  Users and Admins

    Users – Browse Website, looking for accommodations (flats, apartments, houses, 1-2 story houses etc.), READ only
    Admin – Is able to add, delete, view, accommodations and users (Create, Read, Update & Delete)
    Searching & Filtering which accommodations the user wants by number of pets, bedrooms/bathrooms, price, roommates, location etc.
        Detailed description shows up when the user views one of the available accommodation options
        See who the retail agency and agent are
        See if there are nearby shops or schools
    Viewing
        Users can see what houses are available for viewing (open home?)
    Transaction & Checkout systems
        Purchasing an available accommodation
        Email Notifications

Advanced features will also be:

    Chatbot
    Extra Security features

Classes involved:

    User
    Admin
    Retail Agent
    Property (Houses, Apartments, Flats etc.)
    Viewing 
    Transactions

Properties involved:

    UserID, User First & Last Name, User Email
    AdminID, Admin First & Last Name, Admin Email
    AgentID, Agent First & Last Name, Agency Name
    PropertyID, Property Name, Property Address, Property Bedrooms, Property Bathrooms, Property Pets & Property Garage(s), Extended Description
    ViewingID, PropertyID, UserID, Viewing Description, ViewDate
    TransactionID, PropertyID, UserID, Price, UserEmail


Justin:
    -Property model + CRUD
    -Searching and Filtering Logic
    -NearbyAmenities field + property detail view
    -Transaction and Checkout System
    -Email notification on purchase

Ravi:
    -User + Admin models + Login/Registration
    -Role-based access(User = read-only, Admin = full CRUD)
    -Viewing model + open home listings
    -RetailAgent model

Advanced features can be done together or by whoever finishes their section first



