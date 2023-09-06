'''
动态规划.
dp[i][j], A[0:i]变到B[0:j]的距离.
dp[i][j]=min(dp[i-1][j]+1, dp[i][j-1]+1, dp[i-1][j-1] if A[i-1]==B[j-1] else dp[i-1][j-1]+1)
分别对应: dp[i-1][j], 先删除A[i-1], 然后从A[0:i-1]变为B[0:j]
dp[i][j-1]: 先把A[0:i]变为B[0:j-1], 然后加上B[j-1]
dp[i-1][j-1]: 如果末尾A[i-1]==b[j-1], 就只用变前面的; 否则先变前面的, 最后替换最后一个字符.
'''
from numpy import zeros, int32

def EditDistance(A:str, B:str):
    dp = zeros((len(A)+1, len(B)+1), int32)
    dp[0, 0] = 0
    for i in range(1, len(A)+1):
        dp[i,0] = i
    for j in range(1, len(B)+1):
        dp[0,j] = j
    for i in range(1, len(A)+1):
        for j in range(1, len(B)+1):
            options = [
                dp[i-1, j] + 1,
                dp[i, j-1] + 1,
                dp[i-1, j-1] if A[i-1]==B[j-1] else dp[i-1, j-1] + 1
            ]
            dp[i, j] = min(options)

    return dp[-1, -1]


A = input().strip()
B = input().strip()
if len(A) == 0:
    print(len(B))
    exit(0)
elif len(B) == 0:
    print(len(A))
    exit(0)

dp = zeros((len(A)+1, len(B)+1), int32)
dp[0, 0] = 0
for i in range(1, len(A)+1):
    dp[i,0] = i
for j in range(1, len(B)+1):
    dp[0,j] = j
for i in range(1, len(A)+1):
    for j in range(1, len(B)+1):
        options = [
            dp[i-1, j] + 1,
            dp[i, j-1] + 1,
            dp[i-1, j-1] if A[i-1]==B[j-1] else dp[i-1, j-1] + 1
        ]
        dp[i, j] = min(options)
print(dp[-1, -1])