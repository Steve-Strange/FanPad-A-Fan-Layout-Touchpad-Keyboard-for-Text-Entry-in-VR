# 导入包，设置路径
import numpy as np
from openpyxl import Workbook
import os
import json
import csv
from EditDistance import EditDistance
import math
import matplotlib.pyplot as plt

LogDir = "D:/Unity/Project/Keyboard/Assets/ExpLog/"

# 长时间使用. 把 wzt_2,wzt_3和ljh_2,ljh_3排个序. 然后保存到LongtimeUse.xlsx
class WTN:
    # WPM, TER, NCER
    def __init__(self, wpm, ter, ncer) -> None:
        self.wpm = wpm
        self.ter = ter
        self.ncer = ncer

def getKey_WTN(wtn:WTN):
    return wtn.wpm
        

wanted = ['wzt_2', 'wzt_3', 'ljh_2', 'ljh_3', 'dyh_2', 'dyh_3']       # 要导出的数据
data = {}
for i in wanted:
    data[i] = []
for fname in os.listdir(LogDir):
    if not fname.endswith(".json"):
        continue
    info = fname[:-5].split('_')
    name_type = '_'.join(info[:2])
    if name_type in wanted:
        with open(os.path.join(LogDir, fname)) as f:
            content = json.load(f)
        wpm = content["WPM"]
        ter = content["TER"]
        ncer = content["NCER"]
        data[name_type].append(WTN(wpm, ter, ncer))

# 排序并输出
for name_type in wanted:
    data[name_type].sort(key=getKey_WTN)
wb = Workbook()
ws = wb.create_sheet("sorted")
for name_type in wanted:
    row_wpm = [name_type+'_WPM']
    row_ter = [name_type+'_TER']
    row_ncer = [name_type+'_NCER']
    for entity in data[name_type]:
        row_wpm.append(entity.wpm)
        row_ter.append(entity.ter)
        row_ncer.append(entity.ncer)
    ws.append(row_wpm)
    ws.append(row_ter)
    ws.append(row_ncer)
wb.save("Assets/DataProcessing/DataSaved.xlsx")     # 保存到的文件