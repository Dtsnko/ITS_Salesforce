define("ContactPageV2", [], function() {
	return {
		entitySchemaName: "Contact",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		methods: {},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "STRING955264ed-a627-4266-bb77-b26b3e93d1e7",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 13,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "SFDCPhone",
					"enabled": true
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 13
			},
			{
				"operation": "merge",
				"name": "Type44817b1f-178f-4132-8d18-2b6ee074fa02",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 8,
						"layoutName": "ContactGeneralInfoBlock"
					}
				}
			},
			{
				"operation": "move",
				"name": "Type44817b1f-178f-4132-8d18-2b6ee074fa02",
				"parentName": "ContactGeneralInfoBlock",
				"propertyName": "items",
				"index": 15
			},
			{
				"operation": "merge",
				"name": "Owner4a190514-76ce-4ccc-878c-f4c034f19f5b",
				"values": {
					"layout": {
						"colSpan": 12,
						"rowSpan": 1,
						"column": 0,
						"row": 9,
						"layoutName": "ContactGeneralInfoBlock"
					}
				}
			},
			{
				"operation": "merge",
				"name": "Tab88b85ae4TabLabel",
				"values": {
					"order": 3
				}
			},
			{
				"operation": "merge",
				"name": "JobTabContainer",
				"values": {
					"order": 2
				}
			},
			{
				"operation": "merge",
				"name": "HistoryTab",
				"values": {
					"order": 4
				}
			},
			{
				"operation": "merge",
				"name": "NotesAndFilesTab",
				"values": {
					"order": 5
				}
			},
			{
				"operation": "merge",
				"name": "ESNTab",
				"values": {
					"order": 7
				}
			},
			{
				"operation": "merge",
				"name": "EngagementTab",
				"values": {
					"order": 6
				}
			},
			{
				"operation": "move",
				"name": "UsrUnknownContact04689163-01c2-45f4-84e9-4b76fedb017a",
				"parentName": "ContactGeneralInfoBlock",
				"propertyName": "items",
				"index": 16
			},
			{
				"operation": "move",
				"name": "Age",
				"parentName": "ContactGeneralInfoBlock",
				"propertyName": "items",
				"index": 3
			}
		]/**SCHEMA_DIFF*/
	};
});
