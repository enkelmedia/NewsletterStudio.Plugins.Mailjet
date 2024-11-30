# Mailjet Webhooks

Mailjet documentation:

* https://dev.mailjet.com/email/guides/webhooks/
* https://dev.mailjet.com/email/reference/webhook/
* https://dev.mailjet.com/email/reference/send-emails/



## Sending using the API
The Mailjet API will respond with a payload similar to this, the ordering is the same as the order we used to send in the messages.

> NOTE that the status code of the response will be 400+ if ANY of the messages is not successful, however it will still send all others. Aka. the status code is not a reliable way to know if things was ok.

```json
{{
  "Messages": [
    {
      "Status": "success",
      "CustomID": "2f1daac7-1167-432f-8b9b-d0285c80b38b",
      "To": [
        {
          "Email": "lorem1@ipsum.com",
          "MessageUUID": "04fb66ba-ce08-49a9-8284-d4f6324dae65",
          "MessageID": 1152921531826433107,
          "MessageHref": "https://api.mailjet.com/v3/REST/message/1152921531826433107"
        }
      ],
      "Cc": [],
      "Bcc": []
    },
    {
      "Status": "success",
      "CustomID": "2f1daac7-1167-432f-8b9b-d0285c80b38b",
      "To": [
        {
          "Email": "lorem2@ipsum.com",
          "MessageUUID": "289a06b2-aa51-4258-a728-44cdc067b11a",
          "MessageID": 1152921531826433108,
          "MessageHref": "https://api.mailjet.com/v3/REST/message/1152921531826433108"
        }
      ],
      "Cc": [],
      "Bcc": []
    },
    {
      "Status": "error",
      "Errors": [
        {
          "ErrorIdentifier": "face0e23-92f7-4658-9826-0e42abce902b",
          "ErrorCode": "mj-0013",
          "StatusCode": 400,
          "ErrorMessage": "\"foo@test\" is an invalid email address.",
          "ErrorRelatedTo": [
            "To[0].Email"
          ]
        }
      ]
    }
  ]
}}
```
