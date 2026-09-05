-- Run in the studio database as its Entra administrator after replacing the two identity names.
-- Runtime identities can read/write records; schema migration is an operator responsibility.
CREATE USER [<api-managed-identity-name>] FROM EXTERNAL PROVIDER;
CREATE USER [<worker-managed-identity-name>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<api-managed-identity-name>];
ALTER ROLE db_datawriter ADD MEMBER [<api-managed-identity-name>];
ALTER ROLE db_datareader ADD MEMBER [<worker-managed-identity-name>];
ALTER ROLE db_datawriter ADD MEMBER [<worker-managed-identity-name>];
