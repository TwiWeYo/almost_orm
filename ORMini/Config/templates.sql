<table>
	create table <table_name>
	(
		<table_contents>
	);

	<index>
</table>

<index>
	create <unique> index <index_name> on <table_name>
	(
		<index_contents>
	);
</index>

<procedure>
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
</procedure>

<on_conflict_do_nothing>
	on conflict<index> do nothing
</on_conflict_do_nothing>

<on_conflict_update>
	on conflict<index> do
		update set
		<table_contents>;
</on_conflict_update>



