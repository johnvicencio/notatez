using System;
namespace Notatez.Models.Helpers;

public static class EmailTemplateHelper
{
    public static string GetRegistrationEmailTemplate(string name, string email)
    {
        string template = @"<!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Registration Email</title>
                            <style>
                                /* Add your inline CSS styles here */
                                body {
                                    font-family: Arial, sans-serif;
                                    color: #333333;
                                    line-height: 1.5;
                                }
                                .container {
                                    max-width: 600px;
                                    margin: 0 auto;
                                    padding: 20px;
                                    background-color: #f5f5f5;
                                }
                                .heading {
                                    font-size: 24px;
                                    font-weight: bold;
                                    margin-bottom: 20px;
                                }
                                .message {
                                    font-size: 16px;
                                    margin-bottom: 20px;
                                }
                                .footer {
                                    font-size: 14px;
                                    color: #777777;
                                }
                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <h1 class=""heading"">Welcome, " + name + @"!</h1>
                                <p class=""message"">Thank you for registering with Notatez! We're excited to have you on board.</p>
                                <p class=""message"">Your username is: " + email + @".</p>
                                <p class=""message"">Notatez is here to simplify your day-to-day tasks. With our intuitive interface, you can create and organize notes, set reminders, collaborate with others, and access your information from any device.</p>
                                <p class=""message"">As a valued member, you'll enjoy exclusive features and updates. Need assistance? Our support team is ready to help.</p>
                                <p class=""message"">Start exploring Notatez today and unlock the power of effective note-taking.</p>
                                <p class=""message"">Welcome to Notatez!</p>
                                <p class=""footer"">Regards,<br/>Your Notatez Team<br><a href='https://bit.johnvicencio.com'>https://bit.johnvicencio.com</a></p>
                            </div>
                        </body>
                        </html>";

        return template;
    }


    // Add more template methods for other types of emails
}

