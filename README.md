After cloning repo, run the solution by opening a terminal and going into the ```BankingWebApi\BankingWebApi``` directory.
Then run ```docker compose build``` & ```docker compose up```

Note: You need Docker/Docker Desktop to be installed.

In the swagger UI, scroll down to the Authentication controller and run the Authentication: ```/api/authentication/CreateToken``` POST
endpoint. Copy the resulting JWT token.

NOTE: The endpoint assumes any username or password given is correct. For the purposes of this project, I have not implemented
any database or actual verification of username/passwords.

Click the authorize button on the top right of the swagger UI and paste the token and click Authorize.

You should now be able to run all endpoints.