# 重新计算所有json的错误率，并更新.

import numpy as np
import os
import json
from EditDistance import EditDistance

LogDir = "D:/Unity/Project/Keyboard/Assets/ExpLog/"

# 准备一些需要使用的函数.
def TotalError(phrases:list[str], inputSequence: list[str]):
    # 计算输入过程中的总错误数..先还是用下标按顺序来吧..
    allPhrase = "\n".join(phrases).lower()
    cur = ""
    ind = 0
    err = 0
    for next in inputSequence:
        next = next.lower()
        if next == "back":
            cur = cur[:-1]
            ind = 0 if ind == 0 else ind - 1
            next = ""
        elif next == "sym" or next == "shift":
            # 直接忽略
            next = ""
        elif next[0] == '-':
            delete, add = next.split(", ")
            cur = cur[:-(len(delete)-1)] + add[1:]
            ind = ind - len(delete) + 1
            next = add[1:]
        else:
            cur = cur + next
        # 接下来是比较.
        for j in range(len(next)):
            if allPhrase[i] != next[j]:
                if allPhrase[i] == "\n" and next[j] == ' ':
                    i -= 1
                else:
                    err += 1
            i += 1
    return err

# 用back总次数算.
def BackTimes(inputSequence : list[str]):
    err = 0
    for i in inputSequence:
        i = i.lower()
        if i == "back":
            err += 1
    return err

# 计算所有 json 的错误率, 并且更新json.
for fname in os.listdir(LogDir):
    if not fname.endswith(".json"):
        continue
    p = os.path.join(LogDir, fname)
    with open(p, "r") as f:
        content = json.load(f)
    time = content["seconds"]
    length = content["phraseLength"]
    # 算错误率
    phrases = content["phrases"]
    phrases = ' '.join(phrases).lower()
    result = content["result"].split("\n")
    for j in range(len(result)):
        result[j] = result[j].strip()
    result = ' '.join(result).lower()
    # print(phrases)
    # print(result)
    ncerr = EditDistance(phrases, result)
    totalerr = ncerr + BackTimes(content["inputSequence"])
    content["NCER"] = ncerr / length
    content["TER"] = totalerr / length
    with open(p, "w") as f:
        json.dump(content, f, indent=4)