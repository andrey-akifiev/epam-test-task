Feature: StudyGroup

@mytag
Scenario: Sucessfully create a new study group
	When I create a 'new' study group with 'Math' subject
	Then 'new' study group with 'Math' subject has been created

Scenario: Create a new study group with occupied name
   Given I create a 'new' study group with 'Math' subject
	When I create a 'existing' study group with 'Chemistry' subject
	Then 'Conflict' status is returned

Scenario: Create a new study group with occupied subject
   Given I create a 'new' study group with 'Math' subject
	When I create a 'new' study group with 'Math' subject
	Then 'Conflict' status is returned

Scenario: Create a new study group with empty name and subject
	When I create a '' study group with '' subject
	Then 'BadRequest' status is returned

Scenario: When there is no data in database list of study groups should be empty
	When I ask for a list of study groups
	Then the list of study groups 'is empty'

Scenario: When there is data in database list of study groups should have this data
   Given I create a 'new' study group with 'Math' subject
	When I ask for a list of study groups
	Then the list of study groups 'contains new group'

Scenario: Search is resolved with no data when there is no study group
	When I search for a list of study groups by 'Chemistry' subject
	Then the list of study groups 'is empty'

Scenario: Search is resolved with no data when there is no study group with specified subject
   Given I create a 'new' study group with 'Math' subject
	When I search for a list of study groups by 'Chemistry' subject
	Then the list of study groups 'is empty'

Scenario: I'm able to search for a new study group
   Given I create a 'new' study group with 'Math' subject
	When I search for a list of study groups by 'Math' subject
	Then the list of study groups 'contains new group'

Scenario: Search is resolved only with data corresponging to specified subject
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' study group with 'Physics' subject
	When I search for a list of study groups by 'Physics' subject
	Then the list of study groups 'contains new group'

Scenario: Join should not be possible with definitely invalid data
	When I 'join' a '' study group as '' user
	Then 'BadRequest' status is returned
	
Scenario: Join to non-existing study group
   Given I create a 'new' user
	When I 'join' a 'non-existing' study group as 'new' user
	Then 'NotFound' status is returned

Scenario: Join as non-existing user
   Given I create a 'new' study group with 'Chemistry' subject
	When I 'join' a 'new' study group as 'non-existing' user
	Then 'NotFound' status is returned

Scenario: User should be able to join the study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	When I 'join' a 'new' study group as 'new' user
	Then 'OK' status is returned

Scenario: User should not be able to join the study group twice
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	When I 'join' a 'new' study group as 'new' user
	 And I 'join' a 'new' study group as 'new' user
	Then 'Conflict' status is returned

Scenario: User should be able to join several groups
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	 And I 'join' a 'new' study group as 'new' user
	 And I create a 'new' study group with 'Physics' subject
	When I 'join' a 'new' study group as 'new' user
	Then 'OK' status is returned

Scenario: Leave should not be possible with definitely invalid data
	When I 'leave' a '' study group as '' user
	Then 'BadRequest' status is returned

Scenario: Leave non-existing study group
   Given I create a 'new' user
	When I 'leave' a 'non-existing' study group as 'new' user
	Then 'NotFound' status is returned

Scenario: Leave as non-existing user
   Given I create a 'new' study group with 'Chemistry' subject
	When I 'leave' a 'new' study group as 'non-existing' user
	Then 'NotFound' status is returned

Scenario: Leave not assigned study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	When I 'leave' a 'new' study group as 'new' user
	Then 'NotFound' status is returned

Scenario: User should be able to leave the assigned study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	 And I 'join' a 'new' study group as 'new' user
	When I 'leave' a 'new' study group as 'new' user
	Then 'OK' status is returned

Scenario: User should be able to re-join the study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	 And I 'join' a 'new' study group as 'new' user
	 And I 'leave' a 'new' study group as 'new' user
	When I 'join' a 'new' study group as 'new' user
	Then 'OK' status is returned
	 And I ask for a list of study groups
	 And the list of study groups 'contains new group'
	 And the 'new' study group contains 'new' user