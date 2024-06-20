create or replace <procedure_name>
(
	<procedure_contents>
)

language plgsql
as $$
begin
	insert into <table_name>
	(
		<procedure_data>
	)
	values
	(
		<procedure_values>
	)
	<on_conflict>
end;
$$;