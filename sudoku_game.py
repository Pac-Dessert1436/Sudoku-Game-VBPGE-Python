import pygame
from random import shuffle, randint


while True:
    num_blanks = int(input("Enter number of blanks (30-50): "))
    if 30 < num_blanks < 50:
        break
    print("Invalid input. Number of blanks should be within range [30, 50].")


pygame.init()
SCREEN_WIDTH, SCREEN_HEIGHT = 800, 600
screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
pygame.display.set_caption(f"Python Sudoku Game ({num_blanks} blanks)")

BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
CYAN = (0, 255, 255)
BLUE = (0, 0, 255)
YELLOW = (255, 255, 0)

font = pygame.font.SysFont("Times New Roman", 36, True)


def is_sudoku_complete(board: list[list[int]]) -> bool:
    # Check if all cells are filled (no zeros)
    for row in board:
        if 0 in row:
            return False
    # Check each row contains 1-9
    for row in board:
        if sorted(row) != list(range(1, 10)):
            return False
    # Check each column contains 1-9
    for col in range(9):
        column = [board[row][col] for row in range(9)]
        if sorted(column) != list(range(1, 10)):
            return False
    # Check each 3x3 subgrid contains 1-9
    for i in range(0, 9, 3):
        for j in range(0, 9, 3):
            subgrid = []
            for x in range(3):
                for y in range(3):
                    subgrid.append(board[i + x][j + y])
            if sorted(subgrid) != list(range(1, 10)):
                return False
    return True


def create_sudoku_board(num_blanks: int) -> list[list[int]]:
    # Start with a base valid Sudoku board
    sudoku_base = [
        [1, 2, 3, 4, 5, 6, 7, 8, 9],
        [4, 5, 6, 7, 8, 9, 1, 2, 3],
        [7, 8, 9, 1, 2, 3, 4, 5, 6],
        [2, 3, 4, 5, 6, 7, 8, 9, 1],
        [5, 6, 7, 8, 9, 1, 2, 3, 4],
        [8, 9, 1, 2, 3, 4, 5, 6, 7],
        [3, 4, 5, 6, 7, 8, 9, 1, 2],
        [6, 7, 8, 9, 1, 2, 3, 4, 5],
        [9, 1, 2, 3, 4, 5, 6, 7, 8]
    ]
    # Shuffle rows within each band in the board
    for band in range(0, 9, 3):
        rows = sudoku_base[band:band+3]
        shuffle(rows)
        sudoku_base[band:band+3] = rows
    # Shuffle columns within each stack in the board
    for stk in range(0, 9, 3):
        cols = []
        for col in range(stk, stk + 3):
            cols.append([row[col] for row in sudoku_base])
        shuffle(cols)
        for i in range(3):
            current_col = stk + i
            for row in range(9):
                sudoku_base[row][current_col] = cols[i][row]
    # Shuffle numbers by mapping each number to a random permutation
    numbers = list(range(1, 10))
    shuffle(numbers)
    number_map = {original: numbers[original-1] for original in range(1, 10)}
    for row in range(9):
        for col in range(9):
            sudoku_base[row][col] = number_map[sudoku_base[row][col]]

    # Blank out cells in the Sudoku board
    blank_idxs = set()
    while len(blank_idxs) < num_blanks:
        blank_idxs.add((randint(0, 8), randint(0, 8)))
    for x, y in blank_idxs:
        sudoku_base[x][y] = 0
    return sudoku_base


running = True
selected_cell = None
available_number_keys = (
    pygame.K_1, pygame.K_2, pygame.K_3, pygame.K_4, pygame.K_5,
    pygame.K_6, pygame.K_7, pygame.K_8, pygame.K_9
)
sudoku_board = create_sudoku_board(num_blanks)
selectables = [[not j for j in lst] for lst in sudoku_board]

while running:
    for event in pygame.event.get():
        match event.type:
            case pygame.QUIT:
                running = False
            case pygame.MOUSEBUTTONDOWN if not is_sudoku_complete(sudoku_board):
                mouse_x, mouse_y = event.pos
                col = mouse_x // (SCREEN_WIDTH // 9)
                row = mouse_y // (SCREEN_HEIGHT // 9)
                if 0 <= col < 9 and 0 <= row < 9:
                    selected_cell = (row, col)
            case pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False
                elif selected_cell:
                    row, col = selected_cell
                    if event.key in available_number_keys and selectables[row][col]:
                        sudoku_board[row][col] = int(event.unicode)
                        selected_cell = None

    screen.fill(BLACK)

    for i in range(1, 9):
        color, line_w = (BLUE, 1) if i % 3 else (WHITE, 3)
        pygame.draw.line(screen, color, (i * (SCREEN_WIDTH // 9), 0),
                         (i * (SCREEN_WIDTH // 9), SCREEN_HEIGHT), line_w)
        pygame.draw.line(screen, color, (0, i * (SCREEN_HEIGHT // 9)),
                         (SCREEN_WIDTH, i * (SCREEN_HEIGHT // 9)), line_w)

    for row in range(9):
        for col in range(9):
            number = sudoku_board[row][col]
            is_selectable = selectables[row][col]
            selected_color = YELLOW if (row, col) == selected_cell else CYAN
            if number:
                text = font.render(str(number), True,
                                   selected_color if is_selectable else WHITE)
                text_rect = text.get_rect(
                    center=((col + 0.5) * (SCREEN_WIDTH // 9), (row + 0.5) * (SCREEN_HEIGHT // 9)))
                screen.blit(text, text_rect)
            else:
                pygame.draw.circle(screen, selected_color, ((
                    col + 0.5) * (SCREEN_WIDTH // 9), (row + 0.5) * (SCREEN_HEIGHT // 9)), 10)

    if is_sudoku_complete(sudoku_board):
        win_text = font.render("Congratulations! You Win!", True, YELLOW)
        win_rect = win_text.get_rect(
            center=(SCREEN_WIDTH//2, SCREEN_HEIGHT//2))
        pygame.draw.rect(screen, BLACK, win_rect)
        screen.blit(win_text, win_rect)

    pygame.display.flip()

pygame.quit()
