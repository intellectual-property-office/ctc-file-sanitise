Feature: FileSanitiseApiTests

The FileSanitise BDD tests

Scenario: Return sanitised file successfully
	Given A valid file exists
	When apiURL SanitiseFile requested
	Then The sanitised file is returned