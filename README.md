# Notatez
Futurist is a C#-based ASP.NET Core Blazor WebAssembly web app that generates random answers to personal questions about the future. The app's database is a JSON file that stores all possible answers, and the selected answer is displayed on the browser through a logic.

## Problem
The objective is to develop a versatile application capable of performing essential CRUD (Create, Read, Update, Delete) operations. The application should provide a mechanism to store these records persistently, either in a database or other persistent storage solutions. Additionally, the application should incorporate a user authentication component to ensure secure access.

The primary view of the application should display all notes entered by all users, offering a comprehensive overview of the available records. Users should be able to manage these notes, including the ability to create new notes, edit existing ones, update their content, and delete unwanted records. Furthermore, the application should include a dedicated view that exclusively displays notes belonging to the logged-in user, providing a personalized experience.

To enhance usability, the application should incorporate functionalities such as searching, sorting, and pagination. Users should be able to search for specific notes based on content or metadata, facilitating quick retrieval of relevant information. Sorting capabilities will enable users to organize notes based on various criteria, such as date, title, or other relevant attributes. Pagination ensures that large sets of records are divided into manageable chunks, improving navigation and overall performance.

By meeting these requirements, the application will provide users with a powerful and user-friendly environment for managing notes efficiently. The inclusion of database or persistent storage ensures data durability, while the user authentication component guarantees secure access to the application. The ability to perform CRUD operations, along with features like search, sorting, and pagination, contribute to a seamless and productive user experience.

## Solution
Notatez, an ASP.NET MVC C# note application, effectively addresses the stated problem by providing a comprehensive set of features to facilitate note management. Here's how Notatez solves each requirement:

1. CRUD Operations: Notatez enables users to create, read, update, and delete notes. It offers intuitive interfaces and functionalities for performing these operations, allowing users to seamlessly manage their notes.

2. Persistent Storage: Notatez employs an XML-based storage approach, eliminating the need for a full-blown database. This allows users to store their notes persistently and retrieve them whenever needed.

3. Custom Authentication Component: Notatez incorporates a custom authentication component to secure user access. It ensures that only authenticated users can log in and interact with the application, safeguarding their data and privacy.

4. Main View: The primary view of Notatez displays all the notes entered by all users. This provides a comprehensive overview of the available records, allowing users to browse and access notes collectively.

5. Note Management: Notatez offers a dedicated management feature that enables users to create new notes, edit existing ones, update their content, and delete unwanted records. This empowers users with complete control over their notes and ensures efficient note organization.

6. User-Specific View: Notatez includes a separate view that showcases only the notes belonging to the logged-in user. This personalized view allows users to focus on their own notes, enhancing usability and convenience.

7. Search, Sort, and Pagination: Notatez incorporates search functionality, allowing users to search for specific notes based on content or metadata. Sorting capabilities enable users to arrange notes based on various criteria, such as date, title, or other relevant attributes. Pagination ensures that large sets of notes are divided into manageable pages, improving navigation and performance.

By combining these features, Notatez offers a user-friendly note management solution. It provides users with a platform to efficiently create, organize, and access their notes, while ensuring data persistence and security.

## Creating the app
1. Create an ASP.NET Core C# MVC app using the Visual Studio.
2. Name the application Notatez.
3. Create all the necessary interfaces, classes, properties and methods such as Note, INoteService, NoteService, and so on. Create the controllers and the views separately following MVC architecture.
4. Make sure to create the CRUD operations that works with XML elements to be saved as an XML file. There should be two XML files: notes.xml and accounts.xml.
6. Create the views for the different controllers
7. Run the application

## Technologies Used
1. ASP.NET Core MVC
2. C#
3. Bootstrap with Materialize design
5. Content saved on GitHub
6. Deployed on IIS server wint Windows OS

## Using the application
1. Go to the URL: https://notatez.bsite.net
2. Register if you haven't done so
3. Logged in
4. Enter a note, save, edit or delete.
5. Save
6. Logout when done