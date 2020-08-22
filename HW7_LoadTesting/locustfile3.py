import time
import random
import uuid
from locust import HttpUser, task, between
from locust.contrib.fasthttp import FastHttpUser

class QuickstartUser(FastHttpUser):
    wait_time = between(1, 2)

    def get_list_id(self):
        listid_key = random.randrange(5) + 1
        listid = "list0000000000000000000000000000000" + str(listid_key)
        return listid

    def get_lists_ids(self):
        lists = []
        for i in range(2):
            listid_key = random.randrange(5) + 1
            listid = "list0000000000000000000000000000000" + str(listid_key)
            if listid in lists:
                lists.append(listid)
        return lists

    def get_labels_ids(self):
        labels = []
        for i in range(2):
            labelid_key = random.randrange(5) + 1
            labelid = "label000000000000000000000000000000" + str(labelid_key)
            if labelid not in labels:
                labels.append(labelid)
        return labels

    def get_users_ids(self):
        users = []
        for f in range(2):
            userid_key = random.randrange(5) + 1
            userid = "user0000000000000000000000000000000" + str(userid_key)
            if userid not in users:
                users.append(userid)
        return users 
        

    @task
    def create_task(self):
       
        title = "title" + str(random.randrange(100000) + 100)
        listid = self.get_list_id()
        labelsIds = self.get_labels_ids()
        membersIds = self.get_users_ids()       

        reqid = str(uuid.uuid4())

        body = {"title": title, "listid": listid, "membersIds": membersIds, "labelsIds": labelsIds}

        self.client.post(
            "/otusapp/tasks/",
            json=body,
            headers={"X-RequestId": reqid})


    @task(3)
    def get_filtered_tasks(self):
        listsids = self.get_lists_ids()
        labelsIds = self.get_labels_ids()
        membersIds = self.get_users_ids()
        states = []
        for i in range(2):
            states.append(random.randrange(3))
        
        self.client.post(
            "/otusapp/tasks/filter/",
            json={"listsIds": listsids, "usersIds": membersIds, "labelsIds": labelsIds, "states": states}
            )        

    @task(9)
    def get_task(self):
        taskid = self.taskid
        if taskid:
            self.client.get(
                    path="/otusapp/tasks/" + taskid,
                    headers={"X-UserId": self.userid},
                    name="get_task"            
                )  


    @task(5)
    def get_my_tasks(self):
        self.client.get(
                path="/otusapp/tasks/my/",
                headers={"X-UserId": self.userid}            
            )



    def on_start(self):
        userid_key = random.randrange(5) + 1
        userid = "user0000000000000000000000000000000" + str(userid_key)
        self.userid = userid

        response = self.client.get(
                    path="/otusapp/tasks/my/",
                    headers={"X-UserId": self.userid}            
                )
        if response.status_code in [ 200, 202, 204 ]:
            random_task = random.choice(response.json())
            self.taskid = random_task["id"]
        else:
            self.taskid = ""