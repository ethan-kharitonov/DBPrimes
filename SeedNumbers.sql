TRUNCATE TABLE Numbers

SET NOCOUNT ON
DECLARE @n INT = 1
WHILE @n <= 100000
BEGIN 
    INSERT INTO Numbers VALUES (@n)
	SET @n += 1 
END